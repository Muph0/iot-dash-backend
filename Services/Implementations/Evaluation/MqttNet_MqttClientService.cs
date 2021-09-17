using IotDash.Domain;
using IotDash.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services {

    internal class MqttNet_MqttClientService : IHostedMqttClient {

        private class SubscribedTopic : ISubscribedTopic {
            public string Name { get; }
            public int SubscriberCount => subscribers.Count;
            private readonly HashSet<ISubscriber> subscribers = new();

            public SubscribedTopic(string topic) {
                Name = topic;
            }

            public async Task OnMessageReceived(IServiceProvider serviceProvider, MqttApplicationMessageReceivedEventArgs args) {
                foreach (var sub in subscribers) {
                    await sub.OnMessageReceived(new(serviceProvider, args, sub));
                }
            }

            public void Register(ISubscriber subscriber) {
                subscribers.Add(subscriber);
            }

            public void Unregister(ISubscriber subscriber) {
                Debug.Assert(subscribers.Contains(subscriber));

                subscribers.Remove(subscriber);

                if (SubscriberCount == 0) {
                    //Task t = mqtt.Unsubscribe(this.Topic);
                }
            }
        }

        private readonly IMqttClient client;
        private readonly IMqttClientOptions clientOptions;
        private readonly MqttSettings appSettings;
        private readonly ILogger logger;
        private readonly IHostApplicationLifetime lifetime;
        private readonly IHostedExpressionManager interfaceManager;
        private int reconnectionAttempts = 0;
        private readonly Dictionary<string, SubscribedTopic> subscriptions = new();
        private readonly IServiceScopeFactory factory;

        public MqttNet_MqttClientService(
                MqttSettings settings, IMqttClientOptions mqttOptions, ILogger<MqttNet_MqttClientService> logger,
                IMqttClientOptions options, IHostApplicationLifetime lifetime, IHostedExpressionManager interfaceManager,
                IServiceProvider provider) {

            client = new MqttFactory().CreateMqttClient();

            client.ConnectedHandler = this;
            client.DisconnectedHandler = this;
            client.ApplicationMessageReceivedHandler = this;
            this.appSettings = settings;
            this.logger = logger;
            this.clientOptions = options;
            this.lifetime = lifetime;
            this.interfaceManager = interfaceManager;
            this.factory = provider.GetRequiredService<IServiceScopeFactory>();
        }

        public Task StartAsync(CancellationToken cancellationToken) {

            Task.Run(async () => {
                logger.LogDebug($"Establishing connection to broker...");
                try {
                    await client.ConnectAsync(clientOptions, cancellationToken);
                } catch (MQTTnet.Exceptions.MqttCommunicationException ex) {
                    if (!(ex is MQTTnet.Exceptions.MqttCommunicationTimedOutException)) {
                        logger.LogError("Connection error: " + ex.Message);
                    }
                }
            });

            return Task.CompletedTask;
        }


        public async Task<ISubscribedTopic> GetTopic(string topicName) {

            if (subscriptions.TryGetValue(topicName, out var topic)) {
                return topic;
            } else {

                if (client.IsConnected) {
                    try {
                        await Subscribe(topicName);
                    } catch (MqttCommunicationException ex) {
                        // will subscribe on reconnect
                    }
                }

                SubscribedTopic sub = new SubscribedTopic(topicName);
                subscriptions.Add(topicName, sub);
                return sub;
            }
        }

        private async Task Subscribe(params string[] topics) {
            await client.SubscribeAsync(topics.Select(name => new MqttTopicFilter {
                Topic = name,
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
            }).ToArray());
            logger.LogInformation($"Subscribed to {(string.Join(", ", topics.Select(t => $"'{t}'")))} topics.");
        }

        private async Task Unsubscribe(params string[] topics) {
            await client.UnsubscribeAsync(topics);
            logger.LogInformation($"Unsubscribed from {(string.Join(", ", topics.Select(t => $"'{t}'")))} topics.");
        }

        public async Task Publish(string topic, string value) {
            MqttApplicationMessageBuilder msgBuilder = new();
            msgBuilder.WithTopic(topic);
            msgBuilder.WithPayload(Encoding.UTF8.GetBytes(value));
            msgBuilder.WithAtLeastOnceQoS();
            await Publish(msgBuilder.Build());
        }

        Queue<MqttApplicationMessage> messagesToSendQueue = new();
        public async Task Publish(MqttApplicationMessage msg) {
            if (!client.IsConnected) {
                messagesToSendQueue.Enqueue(msg);
                return;
            }

            try {
                await client.PublishAsync(msg);
            } catch (MqttCommunicationException ex) {
                messagesToSendQueue.Enqueue(msg);
            }
        }

        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs) {

            string topic = eventArgs.ApplicationMessage.Topic;
            string payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);

            logger.LogTrace($"Received: '{topic}': '{payload}'");

            if (subscriptions.TryGetValue(topic, out var sub)) {
                using (var scope = factory.CreateScope()) {
                    await sub.OnMessageReceived(scope.ServiceProvider, eventArgs);
                }
            }
        }

        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs) {
            logger.LogInformation($"Connection with broker established.");
            reconnectionAttempts = 0;

            // resubscribe to subscribed topics
            await SubscribeToRegisteredTopics();

            // send queued messages
            await SendQueuedMessages();
        }

        private async Task SubscribeToRegisteredTopics() {
            await this.Subscribe(
                subscriptions.Values.Select(topic => topic.Name).ToArray()
            ); ;
        }

        private async Task SendQueuedMessages() {
            MqttApplicationMessage[]? queueCopy = null;
            lock (messagesToSendQueue) {
                if (messagesToSendQueue.Count > 0) {
                    queueCopy = new MqttApplicationMessage[messagesToSendQueue.Count];
                    messagesToSendQueue.CopyTo(queueCopy, 0);
                    messagesToSendQueue.Clear();
                }
            }
            if (queueCopy != null) {
                foreach (var msg in queueCopy) {
                    await client.PublishAsync(msg);
                }
            }
        }

        public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs) {
            if (appSettings.Broker.ExceededMaxAttempts(reconnectionAttempts)
                || lifetime.ApplicationStopping.IsCancellationRequested) return;

            reconnectionAttempts++;
            logger.LogError($"No connection to MQTT broker, attempting to reconnect... (attempt {reconnectionAttempts})");
            try {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await client.ConnectAsync(clientOptions);
            } catch (Exception e) {
                if (appSettings.Broker.ExceededMaxAttempts(reconnectionAttempts)) {
                    logger.LogCritical(e, $"Connection to MQTT broker failed after {reconnectionAttempts} attempts.");
                    lifetime.StopApplication();
                }
            }
        }
        public async Task StopAsync(CancellationToken cancellationToken) {
            if (client.IsConnected) {
                await client.DisconnectAsync();
            }
        }
    }

}