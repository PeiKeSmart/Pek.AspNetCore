using System.Text;

using NewLife.Log;

using Pek.Configs;

namespace Pek.AspNetCore.WebMiddleware;

/// <summary>
/// 非法字符信息
/// </summary>
public class InvalidCharInfo
{
    public int Position { get; set; }
    public char Character { get; set; }
    public int CharCode { get; set; }
}

/// <summary>
/// 表单数据净化中间件，用于过滤表单中的非法字符
/// 在所有其他处理之前执行，确保在任何表单解析之前净化数据
/// </summary>
public class FormDataSanitizeMiddleware
{
    private readonly RequestDelegate _next;

    public FormDataSanitizeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 检查配置开关，如果未启用则直接跳过
        if (!PekSysSetting.Current.AllowFormDataSanitize)
        {
            // 继续处理请求
            await _next(context).ConfigureAwait(false);
            return;
        }

        var request = context.Request;
        
        // 只处理 POST 请求且 Content-Type 为表单类型的请求
        if (request.Method == "POST" &&
            request.ContentType != null &&
            (request.ContentType.Contains("application/x-www-form-urlencoded") ||
             request.ContentType.Contains("multipart/form-data")))
        {
            try
            {
                // 启用缓冲以便可以多次读取请求体
                request.EnableBuffering();

                // 使用字节级读取，避免字符编码问题
                request.Body.Position = 0;
                
                byte[] bodyBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);
                    bodyBytes = memoryStream.ToArray();
                }

                // XTrace.Log.Info($"[FormDataSanitizeMiddleware] 原始请求体字节长度: {bodyBytes.Length}, 路径: {request.Path}, ContentType: {request.ContentType}");
                
                // 输出原始字节的十六进制表示（前100字节）
                if (bodyBytes.Length > 0)
                {
                    var previewLength = Math.Min(bodyBytes.Length, 100);
                    var hexPreview = Convert.ToHexString(bodyBytes.Take(previewLength).ToArray());
                    // XTrace.Log.Info($"[FormDataSanitizeMiddleware] 原始字节预览(hex): {hexPreview}");
                }

                // 尝试安全地将字节转换为字符串，并检测非法字符
                string body;
                var hasEncodingIssues = false;
                
                try
                {
                    // 使用UTF-8解码，但允许替换无效字符
                    var encoding = new UTF8Encoding(false, false); // 不抛出异常
                    body = encoding.GetString(bodyBytes);
                    
                    // 检查是否有替换字符（通常表示编码问题）
                    if (body.Contains('\uFFFD'))
                    {
                        hasEncodingIssues = true;
                        XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 检测到编码问题，存在替换字符");
                    }
                }
                catch (Exception ex)
                {
                    XTrace.Log.Error($"[FormDataSanitizeMiddleware] 字符串解码失败: {ex.Message}");
                    hasEncodingIssues = true;
                    
                    // 强制使用Latin-1编码（保持字节不变）
                    body = Encoding.GetEncoding("ISO-8859-1").GetString(bodyBytes);
                }

                //XTrace.Log.Info($"[FormDataSanitizeMiddleware] 解码后字符串长度: {body.Length}");
                
                // 输出完整的原始数据（用于调试）
                if (body.Length > 0 && body.Length <= 1000)
                {
                    XTrace.Log.Info($"[FormDataSanitizeMiddleware] 完整原始数据: {body}");
                    
                    // 输出每个问题字符的详细信息
                    var charDetails = new StringBuilder();
                    for (var i = 0; i < Math.Min(body.Length, 200); i++)
                    {
                        var c = body[i];
                        if (c < 32 || c > 126 || c == '\uFFFD')
                        {
                            charDetails.Append($"[{i}]='\\u{((int)c):X4}' ");
                        }
                    }
                    if (charDetails.Length > 0)
                    {
                        XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 发现特殊字符: {charDetails}");
                    }
                }

                // 增强的字符检测
                var invalidChars = FindInvalidCharacters(body);
                if (invalidChars.Count > 0 || hasEncodingIssues)
                {
                    XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 检测到 {invalidChars.Count} 个非法字符或编码问题，开始净化数据，路径: {request.Path}");
                    
                    foreach (var invalidChar in invalidChars)
                    {
                        XTrace.Log.Info($"[FormDataSanitizeMiddleware] 非法字符详情: 位置={invalidChar.Position}, 字符=\\u{invalidChar.CharCode:X4}, ASCII={invalidChar.CharCode}");
                    }
                    
                    // 净化数据
                    var sanitizedBody = SanitizeFormData(body);

                    XTrace.Log.Info($"[FormDataSanitizeMiddleware] 净化后请求体长度: {sanitizedBody.Length}");
                    if (sanitizedBody.Length <= 1000)
                    {
                        XTrace.Log.Info($"[FormDataSanitizeMiddleware] 净化后数据: {sanitizedBody}");
                    }

                    // 创建新的请求体流
                    var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedBody);
                    request.Body = new MemoryStream(sanitizedBytes);
                    request.ContentLength = sanitizedBytes.Length;
                    
                    XTrace.Log.Info($"[FormDataSanitizeMiddleware] 已替换请求体流，新长度: {request.ContentLength}");
                }
                else
                {
                    // 如果没有非法字符，重置流位置
                    request.Body = new MemoryStream(bodyBytes);
                    request.Body.Position = 0;
                    XTrace.Log.Info($"[FormDataSanitizeMiddleware] 未检测到非法字符，路径: {request.Path}，重置流位置到0");
                }
            }
            catch (Exception ex)
            {
                XTrace.Log.Error($"[FormDataSanitizeMiddleware] 表单数据净化过程中发生异常，路径: {request.Path}, 异常: {ex.Message}");
                XTrace.WriteException(ex);
                
                // 重置流位置以确保后续处理正常
                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                }
            }
        }

        // 继续处理请求
        await _next(context).ConfigureAwait(false);
    }

    /// <summary>
    /// 净化表单数据，移除或替换非法字符
    /// </summary>
    private static String SanitizeFormData(String formData)
    {
        if (String.IsNullOrEmpty(formData))
            return formData;

        var removedUrlEncodedNulls = 0;
        var removedDecodedNulls = 0;
        
        // 只处理URL编码的空字符 %00
        var urlEncodedNullPattern = "%00";
        if (formData.Contains(urlEncodedNullPattern))
        {
            var originalLength = formData.Length;
            formData = formData.Replace(urlEncodedNullPattern, ""); // 直接移除URL编码的空字符
            removedUrlEncodedNulls = (originalLength - formData.Length) / 3; // %00 是3个字符
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 移除了 {removedUrlEncodedNulls} 个URL编码的空字符(%00)");
        }
        
        // 只处理已解码字符串中的空字符 \0
        if (formData.Contains('\0'))
        {
            var originalLength = formData.Length;
            formData = formData.Replace("\0", ""); // 移除空字符
            removedDecodedNulls = originalLength - formData.Length;
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 移除了 {removedDecodedNulls} 个已解码的空字符(\\0)");
        }

        var totalRemoved = removedUrlEncodedNulls + removedDecodedNulls;
        if (totalRemoved > 0)
        {
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 总共移除了 {totalRemoved} 个空字符 (URL编码: {removedUrlEncodedNulls}, 已解码: {removedDecodedNulls})");
        }

        return formData;
    }

    /// <summary>
    /// 查找空字符的详细信息（只检查空字符）
    /// </summary>
    private static List<InvalidCharInfo> FindInvalidCharacters(String input)
    {
        var invalidChars = new List<InvalidCharInfo>();
        
        if (String.IsNullOrEmpty(input))
            return invalidChars;

        // 只检查URL编码的空字符 %00
        var urlEncodedNull = "%00";
        var index = 0;
        while ((index = input.IndexOf(urlEncodedNull, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            invalidChars.Add(new InvalidCharInfo
            {
                Position = index,
                Character = '\0',
                CharCode = 0
            });
            XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 发现URL编码的空字符: %00 在位置 {index}");
            index += urlEncodedNull.Length;
        }

        // 只检查已解码的空字符 \0
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == '\0')
            {
                invalidChars.Add(new InvalidCharInfo
                {
                    Position = i,
                    Character = c,
                    CharCode = 0
                });
                XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 发现已解码的空字符: \\0 在位置 {i}");
            }
        }
        
        return invalidChars;
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
    public static IApplicationBuilder UseFormDataSanitize(this IApplicationBuilder app) => app.UseMiddleware<FormDataSanitizeMiddleware>();
}