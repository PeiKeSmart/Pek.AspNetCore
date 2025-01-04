namespace Pek.WebMiddleware;

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
