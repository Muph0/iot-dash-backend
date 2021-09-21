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
        private readonly IHostedEvaluationService interfaceManager;
        private int reconnectionAttempts = 0;
        private readonly Dictionary<string, SubscribedTopic> subscriptions = new();
        private readonly IServiceScopeFactory factory;

        public MqttNet_MqttClientService(
                MqttSettings settings, IMqttClientOptions mqttOptions, ILogger<MqttNet_MqttClientService> logger,
                IMqttClientOptions options, IHostApplicationLifetime lifetime, IHostedEvaluationService interfaceManager,
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
            if (topics.Length == 0) return;
            string message = $"Subscribing to {(string.Join(", ", topics.Select(t => $"'{t}'")))} topic(s): ";
            try {
                var topicFilters = topics.Select(name => new MqttTopicFilter {
                    Topic = name,
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                }).ToArray();
                await client.SubscribeAsync(topicFilters);
                logger.LogDebug(message + "ok.");

            } catch (Exception e) {
                logger.LogError(e, message + "failed.");
            }
        }

        private async Task Unsubscribe(params string[] topics) {
            if (topics.Length == 0) return;
            string message = $"Unsubscribing from {(string.Join(", ", topics.Select(t => $"'{t}'")))} topic(s): ";
            try {
                await client.UnsubscribeAsync(topics);
                logger.LogDebug(message + "ok.");
            } catch (Exception e) {
                logger.LogError(e, message + "failed.");
            }
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
            string message = $"Publishing '{msg.Topic}': '{Encoding.UTF8.GetString(msg.Payload)}' ";
            try {
                await client.PublishAsync(msg);
                logger.LogDebug(message + "ok.");
            } catch (MqttCommunicationException ex) {
                messagesToSendQueue.Enqueue(msg);
            } catch (Exception e) {
                logger.LogError(e, message + "failed.");
            }
        }

        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs) {

            string topic = eventArgs.ApplicationMessage.Topic;
            string payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);

            logger.LogTrace($"Received: '{topic}': '{payload}'");

            try {
                if (subscriptions.TryGetValue(topic, out var sub)) {
                    using (var scope = factory.CreateScope()) {
                        await sub.OnMessageReceived(scope.ServiceProvider, eventArgs);

                        var db = scope.ServiceProvider.GetRequiredService<Data.DataContext>();
                        await db.SaveChangesAsync();
                    }
                }
            } catch (Exception e) {
                logger.LogCritical(e, $"Unhandled exception during MQTT topic event: '{topic}': '{payload}'");
            }
        }

        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs) {
            try {
                logger.LogInformation($"Connection with broker established.");
                reconnectionAttempts = 0;

                // resubscribe to subscribed topics
                await SubscribeToRegisteredTopics();

                // send queued messages
                await SendQueuedMessages();


            } catch (Exception e) {
                logger.LogCritical(e, $"Unhandled exception during MQTT connect event.");
            }
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
            try {
                if (lifetime.ApplicationStopping.IsCancellationRequested) return;

                reconnectionAttempts++;

                string reason = eventArgs.Reason.ToString();
                logger.LogError(eventArgs.Exception, $"Disconnected from MQTT broker ({reason}) attempting to reconnect... (attempt {reconnectionAttempts})");

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
        public async Task StopAsync(CancellationToken cancellationToken) {
            if (client.IsConnected) {
                await client.DisconnectAsync();
            }
        }
    }

}