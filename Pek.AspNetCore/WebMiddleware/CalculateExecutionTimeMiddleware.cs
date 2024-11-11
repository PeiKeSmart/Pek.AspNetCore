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
        if (!ctx.WebSockets.IsWebSocketRequest)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start(); //在下一个中间价处理前，启动计时器

            await _next.Invoke(ctx);

            stopwatch.Stop(); //所有的中间件处理完后，停止秒表。
            XTrace.WriteLine($"请求{ctx.Request.Path}耗时{stopwatch.ElapsedMilliseconds}ms");
        }
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