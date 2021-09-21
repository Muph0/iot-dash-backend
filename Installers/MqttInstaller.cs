using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IotDash.Services;
using MQTTnet.Client.Options;
using IotDash.Settings;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace IotDash.Installers {

    internal class MqttInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            MqttSettings settings = MqttSettings.LoadFrom(configuration);

            services.AddSingleton(settings);

            services.AddSingleton<IHostedEvaluationService, EvaluatorsManager>();
            services.AddHostedService(p => p.GetRequiredService<IHostedEvaluationService>());

            services.AddSingleton<IHostedMqttClient, MqttNet_MqttClientService>();
            services.AddHostedService(p => p.GetRequiredService<IHostedMqttClient>());

            services.AddSingleton(provider => {
                var options = new MqttClientOptionsBuilder();

                options.WithClientId(settings.Client.Id);
                options.WithTcpServer(settings.Broker.Host, settings.Broker.Port);

                if (settings.Credentials != null) {
                    options.WithCredentials(settings.Credentials.UserName, settings.Credentials.Password);
                }

                return options.Build();
            });

        }
    }

}