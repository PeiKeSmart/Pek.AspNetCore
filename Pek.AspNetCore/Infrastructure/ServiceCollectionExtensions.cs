namespace Pek.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加静态编译的Singleton所有单例服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAllSingletons(this IServiceCollection services)
    {
        foreach (var singleton in BaseSingleton.AllSingletons)
        {
            services.AddSingleton(singleton.Key, singleton.Value);
        }
        return services;
    }
}