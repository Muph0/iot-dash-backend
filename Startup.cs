using IotDash.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IotDash.Installers;
using IotDash.Settings;
using System.Diagnostics;
using System.Threading;

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

            app.UseHttpsRedirection();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();

                SwaggerSettings swagg = SwaggerSettings.Load(Configuration);

                app.UseSwagger(options => { options.RouteTemplate = swagg.JsonRoute; });
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint(swagg.UiEndpoint, swagg.Description);
                });
            }

            // build the request pipeline
            {
                app.UseStaticFiles();
                app.UseRouting();
                app.UseMiddleware<ApiErrorReporting>(env);
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints => {
                    endpoints.MapControllers();
                });
            }
        }
    }
}
