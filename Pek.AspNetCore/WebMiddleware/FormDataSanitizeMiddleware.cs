using System.Text;

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

                // 尝试安全地将字节转换为字符串
                string body;
                try
                {
                    // 使用UTF-8解码，但允许替换无效字符
                    var encoding = new UTF8Encoding(false, false); // 不抛出异常
                    body = encoding.GetString(bodyBytes);
                }
                catch (Exception ex)
                {
                    XTrace.Log.Error($"[FormDataSanitizeMiddleware] 字符串解码失败: {ex.Message}, 路径: {request.Path}");
                    // 强制使用Latin-1编码（保持字节不变）
                    body = Encoding.GetEncoding("ISO-8859-1").GetString(bodyBytes);
                }

                // 检查是否包含空字符
                if (ContainsNullCharacters(body))
                {
                    XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 检测到空字符，开始净化数据，路径: {request.Path}");
                    
                    // 净化数据
                    var sanitizedBody = SanitizeFormData(body);

                    // 创建新的请求体流
                    var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedBody);
                    request.Body = new MemoryStream(sanitizedBytes);
                    request.ContentLength = sanitizedBytes.Length;
                    
                    XTrace.Log.Info($"[FormDataSanitizeMiddleware] 成功净化空字符，路径: {request.Path}");
                }
                else
                {
                    // 如果没有空字符，重置流
                    request.Body = new MemoryStream(bodyBytes);
                    request.Body.Position = 0;
                }
            }
            catch (Exception ex)
            {
                XTrace.Log.Error($"[FormDataSanitizeMiddleware] 处理异常，路径: {request.Path}, 异常: {ex.Message}");
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
    /// 快速检查是否包含空字符
    /// </summary>
    private static bool ContainsNullCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // 快速检查URL编码的空字符和实际空字符
        return input.Contains("%00") || input.Contains('\0');
    }

    /// <summary>
    /// 净化表单数据，移除空字符
    /// </summary>
    private static String SanitizeFormData(String formData)
    {
        if (String.IsNullOrEmpty(formData))
            return formData;

        var result = formData;
        var totalRemoved = 0;
        
        // 移除URL编码的空字符 %00
        if (result.Contains("%00"))
        {
            var originalLength = result.Length;
            result = result.Replace("%00", "");
            var removedCount = (originalLength - result.Length) / 3;
            totalRemoved += removedCount;
        }
        
        // 移除已解码的空字符 \0
        if (result.Contains('\0'))
        {
            var originalLength = result.Length;
            result = result.Replace("\0", "");
            totalRemoved += (originalLength - result.Length);
        }

        if (totalRemoved > 0)
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
    public static IApplicationBuilder UseFormDataSanitize(this IApplicationBuilder app) => app.UseMiddleware<FormDataSanitizeMiddleware>();
}