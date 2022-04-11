using IotDash.Utils.Debugging;
using IotDash.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Exceptions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services.Mqtt.Implementation {

    internal class MqttNet_ConnectionHandler : IMqttClientConnectedHandler, IMqttClientDisconnectedHandler, IMqttApplicationMessageReceivedHandler {

        private readonly ILogger<MqttNet_ConnectionHandler> logger;
        private readonly HostedMqttService serviceRoot;
        private readonly IMqttClient client;
        private readonly IMqttClientOptions clientOptions;
        private readonly IHostApplicationLifetime lifetime;
        private readonly MqttSettings appSettings;
        private readonly IServiceScopeFactory factory;
        private int reconnectionAttempts;

        public MqttNet_ConnectionHandler(HostedMqttService serviceRoot, IServiceProvider provider) {
            this.logger = provider.GetRequiredService<ILogger<MqttNet_ConnectionHandler>>();
            this.serviceRoot = serviceRoot;
            this.client = this.serviceRoot.Client;
            this.clientOptions = provider.GetRequiredService<IMqttClientOptions>();
            this.lifetime = provider.GetRequiredService<IHostApplicationLifetime>();
            this.appSettings = provider.GetRequiredService<MqttSettings>();

        }

        async Task IMqttApplicationMessageReceivedHandler.HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs) {
            var msg = eventArgs.ApplicationMessage;
            string topic = msg.Topic;

            logger.LogTrace($"Received: {msg.ToDebugString()}");

            try {
                await serviceRoot.Mediator.Send(topic, serviceRoot, msg);
            } catch (Exception e) {
                logger.LogCritical(e, $"Unhandled exception during MQTT message event: {msg.ToDebugString()}");
            }
        }
        async Task IMqttClientConnectedHandler.HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs) {
            try {
                logger.LogDebug($"Connection with broker established.");
                reconnectionAttempts = 0;

                await ((IMqttClientConnectedHandler)serviceRoot.Publisher).HandleConnectedAsync(eventArgs);
                await ((IMqttClientConnectedHandler)serviceRoot.Subscriber).HandleConnectedAsync(eventArgs);

            } catch (Exception e) {
                logger.LogCritical(e, $"Unhandled exception during MQTT connect event.");
            }
        }
        async Task IMqttClientDisconnectedHandler.HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs) {
            try {
                logger.LogTrace("Connection closed.");
                if (lifetime.ApplicationStopping.IsCancellationRequested) return;

                reconnectionAttempts++;

                if (eventArgs.ClientWasConnected || reconnectionAttempts > 1) {
                    string reason = eventArgs.Reason.ToString();
                    string msg = $"Disconnected from MQTT broker ({reason}) attempting to reconnect... (attempt {reconnectionAttempts})";
                    if (eventArgs.Reason == MqttClientDisconnectReason.NormalDisconnection) {
                        logger.LogWarning(eventArgs.Exception, msg);
                    } else {
                        logger.LogError(eventArgs.Exception, msg);
                    }
                } else {
                    logger.LogInformation("Connecting to MQTT broker.");
                }

                try {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    await client.ConnectAsync(clientOptions);
                } catch (MqttCommunicationException e) {
                    if (appSettings.Broker.ExceededMaxAttempts(reconnectionAttempts)) {
                        logger.LogCritical(e, $"Connection to MQTT broker failed after {reconnectionAttempts} attempts.");
                        lifetime.StopApplication();
                    }
                }


            } catch (Exception e) {
                logger.LogCritical(e, $"Unhandled exception during MQTT disconnect event.");
            }
        }
    }

}