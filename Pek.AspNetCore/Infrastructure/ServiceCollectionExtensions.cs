using NewLife.Model;

namespace Pek.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加自定义DI容器所有单例服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static void AddAllSingletons(this IServiceCollection services)
    {
        //foreach (var kvp in BaseSingleton.AllSingletons)
        //{
        //    var serviceType = kvp.Key;
        //    var implementationInstance = kvp.Value;

        //    if (implementationInstance != null)
        //    {
        //        services.AddSingleton(serviceType, implementationInstance);
        //    }
        //}

        foreach (var item in ObjectContainer.Current.Services)
        {
            var serviceType = item.ServiceType;
            var implementationInstance = item.ImplementationType;

            if (serviceType.GetInterfaces().Contains(typeof(IServiceProvider))) continue;

            if (implementationInstance != null)
            {
                switch (item.Lifetime)
                {
                    case ObjectLifetime.Singleton:
                        services.AddSingleton(serviceType, implementationInstance);
                        break;
                    case ObjectLifetime.Transient:
                        services.AddTransient(serviceType, implementationInstance);
                        break;
                    case ObjectLifetime.Scoped:
                        services.AddScoped(serviceType, implementationInstance);
                        break;
                }
            }
        }

        ObjectContainer.Provider = services.BuildServiceProvider();
    }

    /// <summary>
    /// 获取指定类型的服务对象
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <param name="provider">服务提供者</param>
    /// <returns></returns>
    public static T? GetPekService<T>(this IServiceProvider provider)
    {
        if (provider == null)
        {
            return default;
        }

        return (T?)provider.GetService(typeof(T));
    }
}