using IotDash.Data;
using IotDash.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IotDash.Services.Implementations.ModelStore;
using IotDash.Services.Implementations;

namespace IotDash.Installers {

    internal class DataInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            string connection = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DataContext>(options => {
                options
                .UseLazyLoadingProxies(true)
                .UseChangeTrackingProxies(false)
                .UseMySql(connection, ServerVersion.AutoDetect(connection));
            }, ServiceLifetime.Scoped);

            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<DataContext>();

            // Add model services
            services.AddScoped<IDeviceStore, DeviceEntityStore>();
            services.AddScoped<IInterfaceStore, InterfaceEntityStore>();
            services.AddScoped<IUserStore, UserManagerWrapper>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IHistoryStore, HistoryEntryStore>();

            // Add history service
            services.AddSingleton<IHostedHistoryService, HistoryWritersManager>();
            services.AddHostedService(p => p.GetRequiredService<IHostedHistoryService>());

            // Add settings
            var historySettings = Settings.HistorySettings.LoadFrom(configuration);
            services.AddSingleton(historySettings);
        }
    }

}