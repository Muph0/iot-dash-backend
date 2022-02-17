using IotDash.Utils.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services.Mqtt.Implementation {
    internal sealed class HostedMqttService : IHostedService, IServiceScope {

        private IServiceScope? scope;
        private readonly IServiceScopeFactory factory;
        private readonly ILogger<HostedMqttService> logger;

        public IMqttClient Client { get; }
        public MqttNet_ConnectionHandler Handler { get; }
        public IServiceProvider ServiceProvider => this.scope!.ServiceProvider;
        public MqttNet_Publisher Publisher { get; }
        public MqttNet_Subscriber Subscriber { get; }
        public MqttNet_Mediator Mediator { get; }
        public TaskUnwrapper Unwrapper { get; }


        public HostedMqttService(IServiceProvider provider) {
            this.factory = provider.GetRequiredService<IServiceScopeFactory>();
            this.logger = provider.GetRequiredService<ILogger<HostedMqttService>>();

            Client = new MqttFactory().CreateMqttClient();
            Handler = new MqttNet_ConnectionHandler(this, provider);
            Publisher = new MqttNet_Publisher(this, provider);
            Subscriber = new MqttNet_Subscriber(this, provider);
            Mediator = new MqttNet_Mediator(this, provider);
            Unwrapper = new TaskUnwrapper(provider);

            Client.ConnectedHandler = Handler;
            Client.DisconnectedHandler = Handler;
            Client.ApplicationMessageReceivedHandler = Handler;
        }


        Task IHostedService.StartAsync(CancellationToken cancellationToken) {
            logger.LogDebug("Service starting.");
            scope = this.factory.CreateScope();
            Unwrapper.Start(cancellationToken);
            return Client.DisconnectedHandler.HandleDisconnectedAsync(new(
                clientWasConnected: false,
                exception: null,
                authenticateResult: null,
                reason: MqttClientDisconnectReason.NormalDisconnection
                ));
        }
        async Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            logger.LogDebug("Service stopping.");
            await Unwrapper.Stop();
            if (Client.IsConnected) {
                logger.LogTrace("Closing connection.");
                _ = Client.DisconnectAsync();
            }
            this.Dispose();
        }

        public void Dispose() {
            scope?.Dispose();
            scope = null;
        }
    }

}