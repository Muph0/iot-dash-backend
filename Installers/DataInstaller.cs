using IotDash.Data;
using IotDash.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.AddScoped<IDeviceStore, Services.Implementations.ModelStore.DeviceEntityStore>();
            services.AddScoped<IInterfaceStore, Services.Implementations.ModelStore.InterfaceEntityStore>();
            services.AddScoped<IUserStore, Services.Implementations.ModelStore.UserManagerWrapper>();
            services.AddScoped<IIdentityService, Services.Implementations.IdentityService>();
        }
    }

}