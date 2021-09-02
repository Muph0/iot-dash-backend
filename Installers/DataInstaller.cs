using IotDash.Data;
using IotDash.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IotDash.Installers {

    public class DataInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            string connection = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DataContext>(options => options.UseMySql(connection,
                ServerVersion.AutoDetect(connection)));

            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<DataContext>();

            // Add DeviceStore service
            services.AddScoped<IDeviceStore, EntityDeviceStore>();
            //services.AddSingleton<IDeviceStore, MemoryDeviceStore>();

            // Register identity service
            services.AddScoped<IIdentityService, IdentityService>();
        }
    }

}