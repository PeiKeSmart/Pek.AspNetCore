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
    
    // 缓存常用的字符串和编码对象，避免重复创建
    private static readonly UTF8Encoding SafeUtf8Encoding = new(false, false);
    private static readonly Encoding Latin1Encoding = Encoding.GetEncoding("ISO-8859-1");
    
    // 预编译的Content-Type检查
    private const String FormUrlEncodedType = "application/x-www-form-urlencoded";
    private const String MultipartFormDataType = "multipart/form-data";
    
    // 文件上传检测的常量
    private const String ContentDispositionHeader = "Content-Disposition:";
    private const String ContentTypeHeader = "Content-Type:";
    private const String FilenameParameter = "filename=";

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
        return request.Method == HttpMethods.Post &&
               !request.ContentType.IsNullOrWhiteSpace() &&
               (request.ContentType.Contains(FormUrlEncodedType, StringComparison.OrdinalIgnoreCase) ||
                request.ContentType.Contains(MultipartFormDataType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 处理表单数据请求
    /// </summary>
    private async Task ProcessFormDataRequest(HttpRequest request)
    {
        // 启用缓冲以便可以多次读取请求体
        request.EnableBuffering();
        
        var bodyBytes = await ReadRequestBodyAsync(request).ConfigureAwait(false);
        
        if (bodyBytes.Length == 0)
        {
            return; // 空请求体，无需处理
        }

        var contentType = request.ContentType!;
        
        if (contentType.Contains(FormUrlEncodedType, StringComparison.OrdinalIgnoreCase))
        {
            await ProcessUrlEncodedData(request, bodyBytes).ConfigureAwait(false);
        }
        else if (contentType.Contains(MultipartFormDataType, StringComparison.OrdinalIgnoreCase))
        {
            await ProcessMultipartData(request, bodyBytes).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 读取请求体字节数据
    /// </summary>
    private static async Task<Byte[]> ReadRequestBodyAsync(HttpRequest request)
    {
        request.Body.Position = 0;
        
        using var memoryStream = new MemoryStream();
        await request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 处理 URL 编码的表单数据
    /// </summary>
    private async Task ProcessUrlEncodedData(HttpRequest request, Byte[] bodyBytes)
    {
        var body = DecodeBodySafely(bodyBytes, request.Path);
        
        if (ContainsNullCharacters(body))
        {
            XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 检测到空字符，开始净化数据，路径: {request.Path}");
            
            var sanitizedBody = SanitizeFormData(body);
            await ReplaceRequestBody(request, sanitizedBody).ConfigureAwait(false);
            
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 成功净化空字符，路径: {request.Path}");
        }
        else
        {
            // 重置流位置，无需创建新流
            ResetRequestBody(request, bodyBytes);
        }
    }

    /// <summary>
    /// 处理 multipart 表单数据
    /// </summary>
    private async Task ProcessMultipartData(HttpRequest request, Byte[] bodyBytes)
    {
        // 只分析前几KB来检测文件上传，避免处理整个大文件
        var analysisLength = Math.Min(bodyBytes.Length, 8192); // 分析前8KB
        var analysisBytes = bodyBytes.AsSpan(0, analysisLength);
        var analysisText = SafeUtf8Encoding.GetString(analysisBytes);

        if (ContainsFileUpload(analysisText))
        {
            XTrace.Log.Debug($"[FormDataSanitizeMiddleware] 检测到文件上传，跳过净化处理，路径: {request.Path}");
            ResetRequestBody(request, bodyBytes);
            return;
        }

        // 没有文件上传，检查是否需要净化
        var fullBody = DecodeBodySafely(bodyBytes, request.Path);
        
        if (ContainsNullCharacters(fullBody))
        {
            XTrace.Log.Warn($"[FormDataSanitizeMiddleware] multipart表单检测到空字符，开始净化数据，路径: {request.Path}");
            
            var sanitizedBody = SanitizeFormData(fullBody);
            await ReplaceRequestBody(request, sanitizedBody).ConfigureAwait(false);
            
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 成功净化multipart表单空字符，路径: {request.Path}");
        }
        else
        {
            ResetRequestBody(request, bodyBytes);
        }
    }

    /// <summary>
    /// 安全解码请求体字节数据
    /// </summary>
    private static String DecodeBodySafely(Byte[] bodyBytes, PathString requestPath)
    {
        try
        {
            return SafeUtf8Encoding.GetString(bodyBytes);
        }
        catch (Exception ex)
        {
            XTrace.Log.Error($"[FormDataSanitizeMiddleware] UTF-8解码失败: {ex.Message}, 路径: {requestPath}");
            return Latin1Encoding.GetString(bodyBytes);
        }
    }

    /// <summary>
    /// 重置请求体流
    /// </summary>
    private static void ResetRequestBody(HttpRequest request, Byte[] bodyBytes) => request.Body = new MemoryStream(bodyBytes) { Position = 0 };

    /// <summary>
    /// 替换请求体内容
    /// </summary>
    private static async Task ReplaceRequestBody(HttpRequest request, String newBody)
    {
        var sanitizedBytes = Encoding.UTF8.GetBytes(newBody);
        request.Body = new MemoryStream(sanitizedBytes);
        request.ContentLength = sanitizedBytes.Length;
        await Task.CompletedTask.ConfigureAwait(false); // 保持异步签名一致性
    }

    /// <summary>
    /// 确保流在异常后仍可使用
    /// </summary>
    private static async Task EnsureStreamIsUsable(HttpRequest request)
    {
        try
        {
            if (request.Body.CanSeek)
            {
                request.Body.Position = 0;
            }
        }
        catch (Exception ex)
        {
            XTrace.Log.Error($"[FormDataSanitizeMiddleware] 重置流位置失败: {ex.Message}");
        }
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// 快速检查是否包含空字符
    /// 使用 ReadOnlySpan 提高性能
    /// </summary>
    private static Boolean ContainsNullCharacters(ReadOnlySpan<Char> input)
    {
        if (input.IsEmpty)
            return false;

        // 使用 Span 进行高效查找
        return input.Contains('\0') || input.ToString().Contains("%00", StringComparison.Ordinal);
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
        var lowerContentType = contentType.ToString().ToLowerInvariant();
        
        return !lowerContentType.StartsWith("text/") &&
               !lowerContentType.StartsWith("application/x-www-form-urlencoded") &&
               lowerContentType != "application/json";
    }

    /// <summary>
    /// 净化表单数据，移除空字符
    /// 优化：使用StringBuilder避免多次字符串创建
    /// </summary>
    private static String SanitizeFormData(String formData)
    {
        if (String.IsNullOrEmpty(formData))
            return formData;

        var hasChanges = false;
        var result = formData;
        var totalRemoved = 0;
        
        // 移除URL编码的空字符 %00
        if (result.Contains("%00"))
        {
            var originalLength = result.Length;
            result = result.Replace("%00", String.Empty);
            var removedCount = (originalLength - result.Length) / 3;
            totalRemoved += removedCount;
            hasChanges = true;
        }
        
        // 移除已解码的空字符 \0
        if (result.Contains('\0'))
        {
            var originalLength = result.Length;
            result = result.Replace("\0", String.Empty);
            totalRemoved += (originalLength - result.Length);
            hasChanges = true;
        }

        if (hasChanges && totalRemoved > 0)
        {
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 移除了 {totalRemoved} 个空字符");
        }

        return result;
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