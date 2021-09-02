using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace IotDash.Installers {

    public interface IInstaller {
        void InstallServices(IServiceCollection services, IConfiguration configuration);
    }

    public static class InstallerExtensions {

        public static void InstallServicesInAssembly(this IServiceCollection services, IConfiguration configuration) {

            var installers = typeof(Startup).Assembly.ExportedTypes.Where(ins =>
                    typeof(IInstaller).IsAssignableFrom(ins) &&
                    !ins.IsAbstract && !ins.IsInterface
                ).Select(Activator.CreateInstance)
                .Cast<IInstaller>();

            foreach (var installer in installers) {
                installer.InstallServices(services, configuration);
            }
        }

    }

}