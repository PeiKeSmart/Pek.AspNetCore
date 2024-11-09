using NewLife.Model;

namespace Pek.Infrastructure;

public static class SingletonServiceExtensions
{
    public static void AddAllSingletons(this IServiceCollection services)
    {
        foreach (var kvp in BaseSingleton.AllSingletons)
        {
            var serviceType = kvp.Key;
            var implementationInstance = kvp.Value;

            if (implementationInstance != null)
            {
                services.AddSingleton(serviceType, implementationInstance);
            }
        }

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
    }
}