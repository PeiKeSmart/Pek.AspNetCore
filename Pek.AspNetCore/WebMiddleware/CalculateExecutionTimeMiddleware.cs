using System.Diagnostics;

using NewLife.Log;

using Pek.Configs;
using Pek.Webs;

namespace Pek.WebMiddleware;

/// <summary>
/// 记录执行时间的中间件
/// </summary>
public class CalculateExecutionTimeMiddleware {
    private readonly RequestDelegate _next;  //下一个中间件
    private readonly IWebHelper _webHelper;

    /// <summary>
    /// 实例化
    /// </summary>
    /// <param name="next"></param>
    /// <param name="webHelper"></param>
    public CalculateExecutionTimeMiddleware(RequestDelegate next, IWebHelper webHelper)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        _webHelper = webHelper;
    }

    /// <summary>
    /// 调用
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public async Task Invoke(Microsoft.AspNetCore.Http.HttpContext ctx)
    {
        if (!PekSysSetting.Current.AllowRequestParams)  // 允许获取则执行
        {
            await _next.Invoke(ctx).ConfigureAwait(false);
            return;
        }

        if (ctx.WebSockets.IsWebSocketRequest || IsSignalRRequest(ctx.Request.Path))
        {
            await _next.Invoke(ctx).ConfigureAwait(false);
            return;
        }

        // 检查是否为静态资源
        if (_webHelper.IsStaticResource())
        {
            await _next.Invoke(ctx).ConfigureAwait(false);
            return;
        }

        // 检查请求链接是否包含指定内容
        if (ContainsFilterContent(ctx.Request.Path))
        {
            await _next.Invoke(ctx).ConfigureAwait(false);
            return;
        }

        var stopwatch = Stopwatch.StartNew(); // 启动计时器

        try
        {
            await _next.Invoke(ctx).ConfigureAwait(false); // 调用下一个中间件
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
    private static Boolean ContainsFilterContent(String path)
    {
        var filterContents = new[] { "/greet.Greeter", "/health" }; // 需要过滤的内容
        return filterContents.Any(content => path.Contains(content, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 检查请求路径是否为 SignalR 请求
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static Boolean IsSignalRRequest(String path)
    {
        var signalRPaths = new[] { "/notify-hub" }; // SignalR 请求路径
        return signalRPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// 记录执行时间的中间件扩展
/// </summary>
public static class CalculateExecutionTimeMiddlewareExtensions {
    /// <summary>
    /// 扩展
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IApplicationBuilder UseCalculateExecutionTime(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CalculateExecutionTimeMiddleware>();
    }
}
