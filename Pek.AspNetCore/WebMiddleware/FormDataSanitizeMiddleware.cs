using System.Text;

using NewLife.Log;

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

                // 读取原始请求体
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);

                XTrace.Log.Info($"[FormDataSanitizeMiddleware] 原始请求体长度: {body.Length}, 路径: {request.Path}");

                // 检查是否包含非法字符
                if (ContainsInvalidCharacters(body))
                {
                    XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 检测到非法字符，开始净化数据，路径: {request.Path}");
                    
                    // 净化数据
                    var sanitizedBody = SanitizeFormData(body);

                    XTrace.Log.Info($"[FormDataSanitizeMiddleware] 净化后请求体长度: {sanitizedBody.Length}");

                    // 创建新的请求体流
                    var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedBody);
                    request.Body = new MemoryStream(sanitizedBytes);
                    request.ContentLength = sanitizedBytes.Length;
                }
                else
                {
                    // 如果没有非法字符，重置流位置
                    request.Body.Position = 0;
                    XTrace.Log.Info($"[FormDataSanitizeMiddleware] 未检测到非法字符，路径: {request.Path}");
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
    /// 检查字符串是否包含非法字符
    /// </summary>
    private static Boolean ContainsInvalidCharacters(String input)
    {
        if (String.IsNullOrEmpty(input))
            return false;

        // 检查常见的非法字符
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            // 检查空字符和其他控制字符
            if (c == '\0' || (c >= '\u0001' && c <= '\u001F' && c != '\t' && c != '\n' && c != '\r'))
            {
                XTrace.Log.Warn($"[FormDataSanitizeMiddleware] 发现非法字符: \\u{((Int32)c):X4} 在位置 {i}");
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// 净化表单数据，移除或替换非法字符
    /// </summary>
    private static String SanitizeFormData(String formData)
    {
        if (String.IsNullOrEmpty(formData))
            return formData;

        var result = new StringBuilder(formData.Length);
        var removedCount = 0;
        
        for (var i = 0; i < formData.Length; i++)
        {
            var c = formData[i];
            // 保留可打印字符、制表符、换行符、回车符
            if (c >= 32 || c == '\t' || c == '\n' || c == '\r')
            {
                result.Append(c);
            }
            else
            {
                // 移除控制字符
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            XTrace.Log.Info($"[FormDataSanitizeMiddleware] 移除了 {removedCount} 个非法字符");
        }

        return result.ToString();
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