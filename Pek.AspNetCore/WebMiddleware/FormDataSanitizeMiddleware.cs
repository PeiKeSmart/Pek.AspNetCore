using System.Buffers;
using System.Text;

using NewLife;
using NewLife.Log;

using Pek.Configs;

namespace Pek.AspNetCore.WebMiddleware;

/// <summary>
/// 表单数据净化中间件，用于过滤表单中的非法字符
/// 在所有其他处理之前执行，确保在任何表单解析之前净化数据
/// </summary>
public class FormDataSanitizeMiddleware
{
    private readonly RequestDelegate _next;
    
    private static readonly Encoding Latin1Encoding = Encoding.GetEncoding("ISO-8859-1");
    
    private const String FormUrlEncodedType = "application/x-www-form-urlencoded";
    private const String MultipartFormDataType = "multipart/form-data";
    private const Int32 ScanBufferSize = 4096;
    private const Int32 MultipartHeaderPreviewLength = 8192;
    
    private const String ContentDispositionHeader = "Content-Disposition:";
    private const String ContentTypeHeader = "Content-Type:";
    private const String FilenameParameter = "filename=";
    private const Byte NullByte = 0;
    private const Byte PercentByte = (Byte)'%';
    private const Byte ZeroByte = (Byte)'0';

    public FormDataSanitizeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 检查配置开关，如果未启用则直接跳过
        if (!PekSysSetting.Current.AllowFormDataSanitize)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var request = context.Request;
        
        // 只处理 POST 请求且有Content-Type的表单请求
        if (!IsFormDataRequest(request))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        try
        {
            await ProcessFormDataRequest(request).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            XTrace.Log.Error($"[FormDataSanitizeMiddleware] 处理异常，路径: {request.Path}, 异常: {ex.Message}");
            XTrace.WriteException(ex);
            
            // 确保流可以被后续处理使用
            await EnsureStreamIsUsable(request).ConfigureAwait(false);
        }

        await _next(context).ConfigureAwait(false);
    }

    /// <summary>
    /// 检查是否为需要处理的表单数据请求
    /// </summary>
    private static Boolean IsFormDataRequest(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method) || !request.HasFormContentType)
            return false;

        var contentType = request.ContentType;

        return !contentType.IsNullOrWhiteSpace() &&
               (contentType.Contains(FormUrlEncodedType, StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains(MultipartFormDataType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 处理表单数据请求
    /// </summary>
    private async Task ProcessFormDataRequest(HttpRequest request)
    {
        request.EnableBuffering();

        var contentType = request.ContentType!;
        
        if (contentType.Contains(FormUrlEncodedType, StringComparison.OrdinalIgnoreCase))
        {
            await ProcessUrlEncodedData(request).ConfigureAwait(false);
        }
        else if (contentType.Contains(MultipartFormDataType, StringComparison.OrdinalIgnoreCase))
        {
            await ProcessMultipartData(request).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 处理 URL 编码的表单数据
    /// </summary>
    private async Task ProcessUrlEncodedData(HttpRequest request)
    {
        if (!await ContainsNullMarkersAsync(request).ConfigureAwait(false))
        {
            request.Body.Position = 0;
            return;
        }
        
        XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 检测到空字符，开始净化数据，路径: {request.Path}");

        var totalRemoved = await SanitizeUrlEncodedBodyAsync(request).ConfigureAwait(false);
        if (totalRemoved > 0)
        {
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 成功净化空字符，路径: {request.Path}");
        }
        else
        {
            request.Body.Position = 0;
        }
    }

    /// <summary>
    /// 处理 multipart 表单数据
    /// </summary>
    private async Task ProcessMultipartData(HttpRequest request)
    {
        if (await ContainsFileUploadAsync(request).ConfigureAwait(false))
        {
            XTrace.Log.Debug($"[FormDataSanitizeMiddleware] 检测到文件上传，跳过净化处理，路径: {request.Path}");
            request.Body.Position = 0;
            return;
        }

        if (!await ContainsNullMarkersAsync(request).ConfigureAwait(false))
        {
            request.Body.Position = 0;
            return;
        }

        XTrace.Log.Warn($"[FormDataSanitizeMiddleware] multipart表单检测到空字符，开始净化数据，路径: {request.Path}");

        var totalRemoved = await SanitizeMultipartFormAsync(request).ConfigureAwait(false);
        request.Body.Position = 0;
        if (totalRemoved > 0)
        {
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 成功净化multipart表单空字符，路径: {request.Path}");
        }
    }

    /// <summary>
    /// 按字节流扫描是否包含空字符或 URL 编码的空字符
    /// </summary>
    private static async Task<Boolean> ContainsNullMarkersAsync(HttpRequest request)
    {
        request.Body.Position = 0;

        var buffer = ArrayPool<Byte>.Shared.Rent(ScanBufferSize);
        try
        {
            var previous2 = Byte.MinValue;
            var previous1 = Byte.MinValue;

            while (true)
            {
                var bytesRead = await request.Body.ReadAsync(buffer.AsMemory(0, ScanBufferSize)).ConfigureAwait(false);
                if (bytesRead <= 0)
                    return false;

                for (var index = 0; index < bytesRead; index++)
                {
                    var value = buffer[index];
                    if (value == NullByte)
                        return true;

                    if (previous2 == PercentByte && previous1 == ZeroByte && value == ZeroByte)
                        return true;

                    previous2 = previous1;
                    previous1 = value;
                }
            }
        }
        finally
        {
            ArrayPool<Byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// 净化 URL 编码请求体，仅移除 0x00 和 %00
    /// </summary>
    private static async Task<Int32> SanitizeUrlEncodedBodyAsync(HttpRequest request)
    {
        request.Body.Position = 0;

        var capacity = request.ContentLength is > 0 and <= Int32.MaxValue ? (Int32)request.ContentLength.Value : 0;
        var sanitizedStream = capacity > 0 ? new MemoryStream(capacity) : new MemoryStream();
        var buffer = ArrayPool<Byte>.Shared.Rent(ScanBufferSize);
        var totalRemoved = 0;
        var state = 0;

        try
        {
            while (true)
            {
                var bytesRead = await request.Body.ReadAsync(buffer.AsMemory(0, ScanBufferSize)).ConfigureAwait(false);
                if (bytesRead <= 0)
                    break;

                for (var index = 0; index < bytesRead; index++)
                {
                    var value = buffer[index];
                    switch (state)
                    {
                        case 0:
                            if (value == PercentByte)
                                state = 1;
                            else if (value == NullByte)
                                totalRemoved++;
                            else
                                sanitizedStream.WriteByte(value);
                            break;
                        case 1:
                            if (value == ZeroByte)
                            {
                                state = 2;
                            }
                            else
                            {
                                sanitizedStream.WriteByte(PercentByte);
                                if (value == PercentByte)
                                    state = 1;
                                else if (value == NullByte)
                                {
                                    totalRemoved++;
                                    state = 0;
                                }
                                else
                                {
                                    sanitizedStream.WriteByte(value);
                                    state = 0;
                                }
                            }
                            break;
                        default:
                            if (value == ZeroByte)
                            {
                                totalRemoved++;
                                state = 0;
                            }
                            else
                            {
                                sanitizedStream.WriteByte(PercentByte);
                                sanitizedStream.WriteByte(ZeroByte);
                                if (value == PercentByte)
                                    state = 1;
                                else if (value == NullByte)
                                {
                                    totalRemoved++;
                                    state = 0;
                                }
                                else
                                {
                                    sanitizedStream.WriteByte(value);
                                    state = 0;
                                }
                            }
                            break;
                    }
                }
            }

            if (state == 1)
            {
                sanitizedStream.WriteByte(PercentByte);
            }
            else if (state == 2)
            {
                sanitizedStream.WriteByte(PercentByte);
                sanitizedStream.WriteByte(ZeroByte);
            }

            sanitizedStream.Position = 0;
            request.Body = sanitizedStream;
            request.ContentLength = sanitizedStream.Length;

            if (totalRemoved > 0)
                XTrace.Log.Info($"[FormDataSanitizeMiddleware] 移除了 {totalRemoved} 个空字符");

            return totalRemoved;
        }
        catch
        {
            sanitizedStream.Dispose();
            throw;
        }
        finally
        {
            ArrayPool<Byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// 检测 multipart 请求前部是否包含文件上传特征
    /// </summary>
    private static async Task<Boolean> ContainsFileUploadAsync(HttpRequest request)
    {
        request.Body.Position = 0;

        var buffer = ArrayPool<Byte>.Shared.Rent(MultipartHeaderPreviewLength);
        try
        {
            var bytesRead = await request.Body.ReadAsync(buffer.AsMemory(0, MultipartHeaderPreviewLength)).ConfigureAwait(false);
            if (bytesRead <= 0)
                return false;

            var previewText = Latin1Encoding.GetString(buffer, 0, bytesRead);
            return ContainsFileUpload(previewText.AsSpan());
        }
        finally
        {
            ArrayPool<Byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// 净化 multipart 文本字段，文件部分保持原样
    /// </summary>
    private static async Task<Int32> SanitizeMultipartFormAsync(HttpRequest request)
    {
        request.Body.Position = 0;
        var form = await request.ReadFormAsync().ConfigureAwait(false);

        var sanitizedFields = new Dictionary<String, Microsoft.Extensions.Primitives.StringValues>(form.Count, StringComparer.OrdinalIgnoreCase);
        var totalRemoved = 0;
        var hasChanges = false;

        foreach (var item in form)
        {
            var values = item.Value;
            String[]? sanitizedValues = null;

            for (var index = 0; index < values.Count; index++)
            {
                var currentValue = values[index] ?? String.Empty;
                var sanitizedValue = SanitizeFormData(currentValue, out var removedCount);
                if (removedCount <= 0)
                    continue;

                sanitizedValues ??= CopyStringValues(values);
                sanitizedValues[index] = sanitizedValue;
                totalRemoved += removedCount;
                hasChanges = true;
            }

            sanitizedFields[item.Key] = sanitizedValues == null
                ? values
                : new Microsoft.Extensions.Primitives.StringValues(sanitizedValues);
        }

        if (hasChanges)
        {
            request.Form = new FormCollection(sanitizedFields, form.Files);
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 移除了 {totalRemoved} 个空字符");
        }

        return totalRemoved;
    }

    /// <summary>
    /// 复制表单值并消除空引用
    /// </summary>
    private static String[] CopyStringValues(Microsoft.Extensions.Primitives.StringValues values)
    {
        var result = new String[values.Count];
        for (var index = 0; index < values.Count; index++)
        {
            result[index] = values[index] ?? String.Empty;
        }

        return result;
    }

    /// <summary>
    /// 确保流在异常后仍可使用
    /// </summary>
    private static async Task EnsureStreamIsUsable(HttpRequest request)
    {
        try
        {
            if (request.Body.CanSeek)
                request.Body.Position = 0;
        }
        catch (Exception ex)
        {
            XTrace.Log.Error($"[FormDataSanitizeMiddleware] 重置流位置失败: {ex.Message}");
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// 快速检查是否包含空字符
    /// </summary>
    private static Boolean ContainsNullCharacters(ReadOnlySpan<Char> input)
    {
        if (input.IsEmpty)
            return false;

        if (input.Contains('\0'))
            return true;

        for (var index = 0; index < input.Length - 2; index++)
        {
            if (input[index] == '%' && input[index + 1] == '0' && input[index + 2] == '0')
                return true;
        }

        return false;
    }

    /// <summary>
    /// 检查 multipart/form-data 是否包含文件上传
    /// 优化：只检查必要的部分，使用 Span 避免字符串分配
    /// </summary>
    private static Boolean ContainsFileUpload(ReadOnlySpan<Char> multipartBody)
    {
        if (multipartBody.IsEmpty)
            return false;

        // 逐行检查，避免Split带来的内存分配
        var remaining = multipartBody;
        
        while (!remaining.IsEmpty)
        {
            var lineEnd = remaining.IndexOfAny('\r', '\n');
            var line = lineEnd >= 0 ? remaining[..lineEnd] : remaining;
            
            // 检查 Content-Disposition 是否包含 filename
            if (line.StartsWith(ContentDispositionHeader.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
                line.Contains(FilenameParameter.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            // 检查 Content-Type 是否为非文本类型
            if (line.StartsWith(ContentTypeHeader.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                var contentTypeStart = ContentTypeHeader.Length;
                if (contentTypeStart < line.Length)
                {
                    var contentType = line[contentTypeStart..].Trim();
                    if (IsNonTextContentType(contentType))
                    {
                        return true;
                    }
                }
            }
            
            // 移动到下一行
            if (lineEnd < 0)
                break;
                
            remaining = remaining[(lineEnd + 1)..];
            if (!remaining.IsEmpty && remaining[0] == '\n')
                remaining = remaining[1..];
        }
        
        return false;
    }

    /// <summary>
    /// 检查是否为非文本Content-Type
    /// </summary>
    private static Boolean IsNonTextContentType(ReadOnlySpan<Char> contentType)
    {
         return !contentType.StartsWith("text/".AsSpan(), StringComparison.OrdinalIgnoreCase) &&
             !contentType.StartsWith(FormUrlEncodedType.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
             !contentType.Equals("application/json".AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 净化表单数据，移除空字符
    /// 优化：使用StringBuilder避免多次字符串创建
    /// </summary>
    private static String SanitizeFormData(String formData, out Int32 totalRemoved)
    {
        totalRemoved = 0;

        if (String.IsNullOrEmpty(formData))
            return formData;

        if (!ContainsNullCharacters(formData.AsSpan()))
            return formData;

        var hasChanges = false;
        var result = formData;
        
        if (result.Contains("%00"))
        {
            var originalLength = result.Length;
            result = result.Replace("%00", String.Empty);
            var removedCount = (originalLength - result.Length) / 3;
            totalRemoved += removedCount;
            hasChanges = true;
        }
        
        if (result.Contains('\0'))
        {
            var originalLength = result.Length;
            result = result.Replace("\0", String.Empty);
            totalRemoved += (originalLength - result.Length);
            hasChanges = true;
        }

        return hasChanges ? result : formData;
    }
}

/// <summary>
/// 表单数据净化中间件扩展方法
/// </summary>
public static class FormDataSanitizeExtensions
{
    /// <summary>
    /// 添加表单数据净化中间件
    /// 确保在所有其他处理之前净化表单数据中的非法字符
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseFormDataSanitize(this IApplicationBuilder app) 
        => app.UseMiddleware<FormDataSanitizeMiddleware>();
}