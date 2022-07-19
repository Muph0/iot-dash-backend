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

    /// <summary>
    /// <para>
    /// This class is the root of the MQTT service, responsible for initiation and shutdown of its components.
    /// The whole service is an bridge between the <see cref="IMqttClient"/> and the event bus of the application.
    /// <br />
    /// The service consists of the following components:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="MqttNet_ConnectionHandler"/></item>
    /// <item><see cref="MqttNet_Mediator"/></item>
    /// <item><see cref="MqttNet_Publisher"/></item>
    /// <item><see cref="MqttNet_Subscriber"/></item>
    /// </list>
    /// </summary>
    internal sealed class HostedMqttService : IHostedService, IServiceScope {

        private IServiceScope? scope;
        private readonly IServiceScopeFactory factory;
        private readonly ILogger<HostedMqttService> logger;

        /// <summary>
        /// Dependency injection service provider.
        /// </summary>
        public IServiceProvider ServiceProvider => this.scope!.ServiceProvider;

        /// <summary>
        /// MQTT library-specific client.
        /// </summary>
        public IMqttClient Client { get; }
        /// <summary>
        /// Receives events from <see cref="Client"/> and sends them to other components of this service.
        /// </summary>
        public MqttNet_ConnectionHandler Handler { get; }
        /// <summary>
        /// Queues ready-to-send messages and passes them to <see cref="Client"/>.
        /// </summary>
        public MqttNet_Publisher Publisher { get; }
        /// <summary>
        /// Makes sure
        /// </summary>
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