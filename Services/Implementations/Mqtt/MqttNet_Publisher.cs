using IotDash.Utils.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace IotDash.Services.Mqtt.Implementation {
    internal class MqttNet_Publisher : IMqttClientConnectedHandler {
        private readonly ILogger logger;
        private readonly IMqttClient client;
        Queue<MqttApplicationMessage> messagesToSendQueue = new();

        public MqttNet_Publisher(HostedMqttService serviceRoot, IServiceProvider provider) {
            logger = provider.GetRequiredService<ILogger<MqttNet_Publisher>>();
            
            client = serviceRoot.Client;
        }

        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs) {
            while (messagesToSendQueue.Count != 0) {
                await SendAsync(messagesToSendQueue.Dequeue());
            }
        }

        public async Task SendAsync(string topic, string value) {
            MqttApplicationMessageBuilder msgBuilder = new();
            msgBuilder.WithTopic(topic);
            msgBuilder.WithPayload(Encoding.UTF8.GetBytes(value));
            msgBuilder.WithAtLeastOnceQoS();
            await SendAsync(msgBuilder.Build());
        }

        public async Task SendAsync(MqttApplicationMessage msg) {
            if (!client.IsConnected) {
                messagesToSendQueue.Enqueue(msg);
                return;
            }

            try {
                await client.PublishAsync(msg);
                logger.LogDebug(msg.ToDebugString() + " ok.");
            } catch (MqttCommunicationException ex) {
                messagesToSendQueue.Enqueue(msg);
            } catch (Exception e) {
                logger.LogError(e, msg.ToDebugString() + " failed.");
            }
        }

    }

}