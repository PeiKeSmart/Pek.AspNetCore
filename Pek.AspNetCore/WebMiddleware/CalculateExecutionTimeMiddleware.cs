using System.Diagnostics;

using NewLife.Log;

namespace Pek.WebMiddleware;

/// <summary>
/// 记录执行时间的中间件
/// </summary>
public class CalculateExecutionTimeMiddleware
{
    private readonly RequestDelegate _next;  //下一个中间件
    Stopwatch? stopwatch;

    /// <summary>
    /// 实例化
    /// </summary>
    /// <param name="next"></param>
    public CalculateExecutionTimeMiddleware(RequestDelegate next) => _next = next ?? throw new ArgumentNullException(nameof(next));

    /// <summary>
    /// 调用
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext ctx)
    {
        if (ctx.WebSockets.IsWebSocketRequest)
        {
            await _next.Invoke(ctx);
            return;
        }

        // 检查是否为静态资源
        if (WebHelperEx.IsStaticResource(ctx))
        {
            await _next.Invoke(ctx);
            return;
        }

        // 检查请求链接是否包含指定内容
        if (ContainsFilterContent(ctx.Request.Path))
        {
            await _next.Invoke(ctx);
            return;
        }

        var stopwatch = Stopwatch.StartNew(); // 启动计时器

        try
        {
            await _next.Invoke(ctx); // 调用下一个中间件
        }
        finally
        {
            stopwatch.Stop(); // 确保在所有情况下都停止计时器
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            // 记录请求耗时
            XTrace.WriteLine($"请求{ctx.Request.Path}耗时{elapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// 检查请求链接是否包含指定内容
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private Boolean ContainsFilterContent(String path)
    {
        var filterContents = new[] { "/greet.Greeter" }; // 需要过滤的内容
        return filterContents.Any(content => path.Contains(content, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// 记录执行时间的中间件扩展
/// </summary>
public static class CalculateExecutionTimeMiddlewareExtensions
{
    /// <summary>
    /// 扩展
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IApplicationBuilder UseCalculateExecutionTime(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }
        return app.UseMiddleware<CalculateExecutionTimeMiddleware>();
    }
}