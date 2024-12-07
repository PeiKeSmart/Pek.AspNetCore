using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NewLife.Model;

using Pek.ResumeFileResult;
using Pek.ResumeFileResult.Executor;

namespace Pek.Infrastructure;

/// <summary>
/// 依赖注入ServiceCollection容器扩展方法
/// </summary>
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
    /// 注入断点续传服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddResumeFileResult(this IServiceCollection services)
    {
        services.TryAddSingleton<IActionResultExecutor<ResumePhysicalFileResult>, ResumePhysicalFileResultExecutor>();
        services.TryAddSingleton<IActionResultExecutor<ResumeVirtualFileResult>, ResumeVirtualFileResultExecutor>();
        services.TryAddSingleton<IActionResultExecutor<ResumeFileStreamResult>, ResumeFileStreamResultExecutor>();
        services.TryAddSingleton<IActionResultExecutor<ResumeFileContentResult>, ResumeFileContentResultExecutor>();
        return services;
    }


}