using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using IotDash.Installers;
using IotDash.Settings;
using IotDash.Controllers.V1;
using IotDash.Contracts.V1;

namespace IotDash {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            try {
                services.InstallServicesInAssembly(Configuration);
            } catch (Exception ex) {
                var logger = new Services.MyConsoleLoggerProvider().CreateLogger(nameof(Startup));

                logger.LogCritical(ex, "Exception thrown during configuration.");
                Environment.Exit(1);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();

                SwaggerSettings swagg = SwaggerSettings.LoadFrom(Configuration);

                app.UseSwagger(options => { options.RouteTemplate = swagg.JsonRoute; });
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint(swagg.UiEndpoint, swagg.Description);
                });
            }

            // build the request pipeline
            {
                app.UseStaticFiles();
                app.UseCors(nameof(MvcInstaller.CorsPolicy));
                app.UseRouting();
                app.UseMiddleware<ApiErrorReporting>(env);
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints => {
                    endpoints.MapControllers();
                    endpoints.MapHub<ChartHub>(ApiRoutes.Interface.ReadValue);
                });
            }
        }
    }
}
