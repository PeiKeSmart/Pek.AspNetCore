namespace Pek.Infrastructure;

/// <summary>
/// 表示应用程序启动时配置服务和中间件的对象
/// </summary>
public interface IPekStartup: IDHStartup
{
    /// <summary>
    /// 添加并配置任何中间件
    /// </summary>
    /// <param name="services">服务描述符集合</param>
    /// <param name="configuration">应用程序的配置</param>
    /// <param name="webHostEnvironment"></param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment);

    /// <summary>
    /// 配置添加的中间件的使用
    /// </summary>
    /// <param name="application">用于配置应用程序的请求管道的生成器</param>
    void Configure(IApplicationBuilder application);

    /// <summary>
    /// 注册路由
    /// </summary>
    /// <param name="endpoints">路由生成器</param>
    void UseDHEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>
    /// 将区域路由写入数据库
    /// </summary>
    void ConfigureArea();

    /// <summary>
    /// 调整菜单
    /// </summary>
    void ChangeMenu();

    /// <summary>
    /// 配置使用添加的中间件
    /// </summary>
    /// <param name="application">用于配置应用程序的请求管道的生成器</param>
    void ConfigureMiddleware(IApplicationBuilder application);

    /// <summary>
    /// UseRouting前执行的数据
    /// </summary>
    /// <param name="application"></param>
    void BeforeRouting(IApplicationBuilder application);

    /// <summary>
    /// UseAuthentication或者UseAuthorization后面 Endpoints前执行的数据
    /// </summary>
    /// <param name="application"></param>
    void AfterAuth(IApplicationBuilder application);

    /// <summary>
    /// 获取此启动配置实现的顺序。主要针对ConfigureMiddleware、UseRouting前执行的数据、UseAuthentication或者UseAuthorization后面 Endpoints前执行的数据
    /// </summary>
    Int32 ConfigureOrder { get; }
}

public class DHConast
{
    /// <summary>IDHStartup集合</summary>
    public static IEnumerable<IDHStartup>? DHStartups { get; set; }
}