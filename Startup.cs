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
using IotDash.Utils;
using System.IO;
using IotDash.Middleware;

namespace IotDash {


    /// <summary>
    /// This class is responsible for configuring all service and installing them into the container.
    /// It also configures the HTTP request pipeline.
    /// </summary>
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }



        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Container into which services are installed.</param>
        public void ConfigureServices(IServiceCollection services) {
            try {
                services.InstallServicesInAssembly(Configuration);
            } catch (Exception ex) {
                var logger = new Services.MyConsoleLoggerProvider().CreateLogger(nameof(Startup));

                logger.LogCritical(ex, "Exception thrown during configuration.");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// This method gets called by the runtime.
        /// HTTP request pipeline is configured in this method.
        /// </summary>
        /// <param name="app">Provides mechanisms to configure application request pipelies.</param>
        /// <param name="env">Provides information about the hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {

            if (env.IsDevelopment()) {
                Console.WriteLine($"Hosting static files from \"{env.WebRootPath}\".");
                Console.WriteLine("Development mode enabled.");
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
                    endpoints.MapHub<EventHub>();
                    endpoints.MapFallbackToFile("/index.html");
                });
            }
        }
    }
}
