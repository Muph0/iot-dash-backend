using IotDash.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash {
    public class Program {

        public static async Task<int> Main(string[] args) {

            var logger = new MyConsoleLoggerProvider().CreateLogger("Program.Main");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            IHost? host = null;

            try {
                host = CreateHostBuilder(args).Build();
            } catch (Exception ex) {
                logger.LogCritical(ex, "Exception thrown during initialization.");
                return 1;
            }

            if (host == null) {
                return 1;
            }

            try {
                await host.RunAsync();
            } catch (Exception ex) {
                logger.LogCritical(ex, "Unhandled exception:");
                return 1;
            }

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => {
                    logging.ClearProviders();
                    logging.AddProvider(new MyConsoleLoggerProvider());
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
