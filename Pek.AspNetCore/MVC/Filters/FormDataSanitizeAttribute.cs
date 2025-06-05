using System.Text;

using Microsoft.AspNetCore.Mvc.Filters;

using NewLife.Log;

namespace Pek.AspNetCore.MVC.Filters;

/// <summary>
/// 表单数据净化特性，用于过滤表单中的非法字符
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class FormDataSanitizeAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        
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

                // 重置流位置
                request.Body.Position = 0;

                // 检查是否包含非法字符
                if (ContainsInvalidCharacters(body))
                {
                    // 净化数据
                    var sanitizedBody = SanitizeFormData(body);

                    // 创建新的请求体流
                    var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedBody);
                    request.Body = new MemoryStream(sanitizedBytes);
                    request.ContentLength = sanitizedBytes.Length;
                }
                else
                {
                    // 如果没有非法字符，重置流位置
                    request.Body.Position = 0;
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不阻止请求继续执行
                // 这里可以根据需要添加日志记录
                // Logger?.LogWarning(ex, "表单数据净化过程中发生异常");
                XTrace.Log.Warn("表单数据净化过程中发生异常");
                XTrace.WriteException(ex);
                
                // 重置流位置以确保后续处理正常
                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                }
            }
        }

        // 继续执行 Action
        await next().ConfigureAwait(false);
    }

    /// <summary>
    /// 检查字符串是否包含非法字符
    /// </summary>
    private static Boolean ContainsInvalidCharacters(String input)
    {
        if (String.IsNullOrEmpty(input))
            return false;

        // 检查常见的非法字符
        return input.Contains('\0') || 
               input.Contains('\u0001') || 
               input.Contains('\u0002');
    }

    /// <summary>
    /// 净化表单数据，移除或替换非法字符
    /// </summary>
    private static String SanitizeFormData(String formData)
    {
        if (String.IsNullOrEmpty(formData))
            return formData;

        // 移除空字符和其他控制字符
        var sanitized = formData
            .Replace("\0", "") // 移除空字符
            .Replace("\u0001", "") // 移除 SOH 字符
            .Replace("\u0002", "") // 移除 STX 字符
            .Replace("\u0003", "") // 移除 ETX 字符
            .Replace("\u0004", "") // 移除 EOT 字符
            .Replace("\u0005", "") // 移除 ENQ 字符
            .Replace("\u0006", "") // 移除 ACK 字符
            .Replace("\u0007", "") // 移除 BEL 字符
            .Replace("\u0008", "") // 移除 BS 字符
            .Replace("\u000B", "") // 移除 VT 字符
            .Replace("\u000C", "") // 移除 FF 字符
            .Replace("\u000E", "") // 移除 SO 字符
            .Replace("\u000F", "") // 移除 SI 字符
            .Replace("\u0010", "") // 移除 DLE 字符
            .Replace("\u0011", "") // 移除 DC1 字符
            .Replace("\u0012", "") // 移除 DC2 字符
            .Replace("\u0013", "") // 移除 DC3 字符
            .Replace("\u0014", "") // 移除 DC4 字符
            .Replace("\u0015", "") // 移除 NAK 字符
            .Replace("\u0016", "") // 移除 SYN 字符
            .Replace("\u0017", "") // 移除 ETB 字符
            .Replace("\u0018", "") // 移除 CAN 字符
            .Replace("\u0019", "") // 移除 EM 字符
            .Replace("\u001A", "") // 移除 SUB 字符
            .Replace("\u001B", "") // 移除 ESC 字符
            .Replace("\u001C", "") // 移除 FS 字符
            .Replace("\u001D", "") // 移除 GS 字符
            .Replace("\u001E", "") // 移除 RS 字符
            .Replace("\u001F", ""); // 移除 US 字符

        return sanitized;
    }
}