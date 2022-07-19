using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IotDash.Installers {

    /// <summary>
    /// Represents a contract for service installer.
    /// Assembly-wide scan for implementations of this interface is performed on <see cref="Startup.ConfigureServices(IServiceCollection)"/>.
    /// The found implementations are instantiated and <see cref="IInstaller.InstallServices(IServiceCollection, IConfiguration)"/> is invoked on them.
    /// See Architecture in programmers manual.
    /// </summary>
    public interface IInstaller {

        /// <summary>
        /// Configure and register services installed by this installer.
        /// </summary>
        /// <param name="services">Container into which services are installed.</param>
        /// <param name="configuration">Key/value application configuration from <c>appconfig.json</c>.</param>
        void InstallServices(IServiceCollection services, IConfiguration configuration);
    }

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

        public static IEnumerable<TypeInfo> AddAllImplementationsInAssembly<TServiceType>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped) {

            Type serviceType = typeof(TServiceType);

            var implementations = typeof(Startup).Assembly.DefinedTypes.Where(impl =>
                    serviceType.IsAssignableFrom(impl) &&
                    !impl.IsAbstract && !impl.IsInterface
                );

            foreach (Type implType in implementations) {

                services.Add(new(implType, implType, lifetime));
                services.Add(new(serviceType, p => p.GetRequiredService(implType), lifetime));

                if (implType.IsAssignableTo(typeof(IHostedService))) {
                    services.Add(new(typeof(IHostedService), p => p.GetRequiredService(implType), lifetime));
                }
            }

            return implementations;
        }

        public static void Replace<TServiceType, TImplementationType>(this IServiceCollection services) {

            var oldService = services.Single(s => s.ServiceType == typeof(TServiceType));

            services.Remove(oldService);
            services.Add(new (oldService.ServiceType, typeof(TImplementationType), oldService.Lifetime));

        }
    }

}