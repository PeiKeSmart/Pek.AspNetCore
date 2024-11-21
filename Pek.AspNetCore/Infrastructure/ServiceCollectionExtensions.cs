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
        if (provider == null) return default;

        //// 服务类是否当前类的基类
        //if (provider.GetType().As<T>()) return (T)provider;

        return (T?)provider.GetService(typeof(T));
    }

    /// <summary>获取必要的服务，不存在时抛出异常</summary>
    /// <param name="provider">服务提供者</param>
    /// <param name="serviceType">服务类型</param>
    /// <returns></returns>
    public static Object GetPekRequiredService(this IServiceProvider provider, Type serviceType)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

        return provider.GetService(serviceType) ?? throw new InvalidOperationException($"Unregistered type {serviceType.FullName}");
    }

    /// <summary>获取必要的服务，不存在时抛出异常</summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <param name="provider">服务提供者</param>
    /// <returns></returns>
    public static T GetPekRequiredService<T>(this IServiceProvider provider) => provider == null ? throw new ArgumentNullException(nameof(provider)) : (T)provider.GetPekRequiredService(typeof(T));

    /// <summary>获取一批服务</summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <param name="provider">服务提供者</param>
    /// <returns></returns>
    public static IEnumerable<T> GetPekServices<T>(this IServiceProvider provider) => provider.GetPekServices(typeof(T)).Cast<T>();

    /// <summary>获取一批服务</summary>
    /// <param name="provider">服务提供者</param>
    /// <param name="serviceType">服务类型</param>
    /// <returns></returns>
    public static IEnumerable<Object> GetPekServices(this IServiceProvider provider, Type serviceType)
    {
        //var sp = provider as ServiceProvider;
        //if (sp == null && provider is MyServiceScope scope) sp = scope.MyServiceProvider as ServiceProvider;
        //var sp = provider.GetService<ServiceProvider>();
        //if (sp != null && sp.Container is ObjectContainer ioc)
        var ioc = GetPekService<ObjectContainer>(provider);
        if (ioc != null)
        {
            //var list = new List<Object>();
            //foreach (var item in ioc.Services)
            //{
            //    if (item.ServiceType == serviceType) list.Add(ioc.Resolve(item, provider));
            //}
            for (var i = ioc.Services.Count - 1; i >= 0; i--)
            {
                var item = ioc.Services[i];
                if (item.ServiceType == serviceType) yield return ioc.Resolve(item, provider);
            }
            //return list;
        }
        else
        {
            var serviceType2 = typeof(IEnumerable<>)!.MakeGenericType(serviceType);
            var enums = (IEnumerable<Object>)provider.GetPekRequiredService(serviceType2);
            foreach (var item in enums)
            {
                yield return item;
            }
        }
    }

}