namespace Pek.WebMiddleware;

/// <summary>
/// 全局错误处理中间件
/// </summary>
public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;  //下一个中间件

    /// <summary>
    /// 实例化
    /// </summary>
    /// <param name="next"></param>
    public GlobalErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// 调用
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next.Invoke(ctx).ConfigureAwait(false); // 调用下一个中间件
        }
        catch (Exception ex)
        {
            // 捕获到未处理的异常
            await HandleExceptionAsync(ctx, ex).ConfigureAwait(false);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is InvalidDataException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else if (exception is BadHttpRequestException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        // 如果需要继续向上传递异常，可以重新抛出
        throw exception;
    }
}

/// <summary>
/// 记录执行时间的中间件扩展
/// </summary>
public static class GlobalErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// 扩展
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<GlobalErrorHandlingMiddleware>();
    }
}
