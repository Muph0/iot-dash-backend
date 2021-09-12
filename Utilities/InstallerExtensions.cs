using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace IotDash.Installers {
    public static class InstallerExtensions {

        public static void InstallServicesInAssembly(this IServiceCollection services, IConfiguration configuration) {

            var installers = typeof(Startup).Assembly.DefinedTypes.Where(ins =>
                    typeof(IInstaller).IsAssignableFrom(ins) &&
                    !ins.IsAbstract && !ins.IsInterface
                ).Select(Activator.CreateInstance)
                .Cast<IInstaller>();

            foreach (var installer in installers) {
                installer.InstallServices(services, configuration);
            }
        }

        public static void AddAllImplementationsInAssembly<TServiceType>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped) {

            Type serviceType = typeof(TServiceType);

            var implementations = typeof(Startup).Assembly.DefinedTypes.Where(impl =>
                    serviceType.IsAssignableFrom(impl) &&
                    !impl.IsAbstract && !impl.IsInterface
                );

            foreach (Type implType in implementations) {
                services.Add(new(serviceType, implType, lifetime));
            }
        }

        public static void Replace<TServiceType, TImplementationType>(this IServiceCollection services) {

            var oldService = services.Single(s => s.ServiceType == typeof(TServiceType));

            services.Remove(oldService);
            services.Add(new ServiceDescriptor(oldService.ServiceType, typeof(TImplementationType), oldService.Lifetime));

        }
    }
}