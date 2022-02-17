using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet.Client.Options;
using IotDash.Settings;
using Microsoft.Extensions.Hosting;
using System.Linq;
using IotDash.Services.Messaging.Implementation;
using IotDash.Services.Mqtt;
using IotDash.Services.Mqtt.Implementation;
using IotDash.Domain.Mediator;

namespace IotDash.Installers {

    internal class MqttInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            ////////////////
            // Mqtt client options & app settings

            MqttSettings settings = MqttSettings.LoadFrom(configuration);
            services.AddSingleton(settings);

            services.AddSingleton(provider => {
                var options = new MqttClientOptionsBuilder();

                options.WithClientId(settings.Client.Id);
                options.WithTcpServer(settings.Broker.Host, settings.Broker.Port);

                if (settings.Credentials != null) {
                    options.WithCredentials(settings.Credentials.UserName, settings.Credentials.Password);
                }

                return options.Build();
            });

            ////////////////
            // Mqtt Client service

            // old
            //services.AddSingleton<IHostedMqttClient, MqttNetMqttClientService>();
            //services.AddHostedService(p => p.GetRequiredService<IHostedMqttClient>());

            // new
            services.AddSingleton<HostedMqttService>();
            services.AddHostedService(p => p.GetRequiredService<HostedMqttService>());
            services.AddSingleton<AMqttMediator>(p => p.GetRequiredService<HostedMqttService>().Mediator);
        }
    }
}