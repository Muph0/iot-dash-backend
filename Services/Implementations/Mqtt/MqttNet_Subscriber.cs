using IotDash.Utils.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services.Mqtt.Implementation {

    /// <summary>
    /// A component of the <see cref="HostedMqttService"/>.
    /// This class is responsible for maintaining list of subscribed topics synchronised with the MQTT broker,
    /// even in case of temporary disconnection.
    /// </summary>
    internal class MqttNet_Subscriber : IMqttClientConnectedHandler {

        private readonly ILogger logger;
        private readonly IMqttClient client;

        private readonly SemaphoreSlim sem = new(1, 1);
        HashSet<string> subscribedTopics = new();

        public MqttNet_Subscriber(HostedMqttService serviceRoot, IServiceProvider provider) {
            logger = provider.GetRequiredService<ILogger<MqttNet_Subscriber>>();
            client = serviceRoot.Client;
        }

        async Task IMqttClientConnectedHandler.HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs) {
            string[] lostTopics;

            using (await sem.LockAsync()) {
                lostTopics = subscribedTopics.ToArray();
                subscribedTopics.Clear();
            }

            await Subscribe(lostTopics);
        }

        /// <summary>
        /// Add a topic to subscription list and potentially update with the broker.
        /// </summary>
        /// <param name="topic">The topic name.</param>
        /// <returns>Task that completes when the communication with broker is done.</returns>
        public Task Subscribe(string topic) => Subscribe(new[] { topic });
        /// <summary>
        /// Add topics to subscription list and potentially update with the broker.
        /// </summary>
        /// <param name="topics">List of the topic names.</param>
        /// <returns>Task that completes when the communication with broker is done.</returns>
        public async Task Subscribe(IEnumerable<string> topics) {
            using (await sem.LockAsync()) {

                topics = topics.Where(t => subscribedTopics.Add(t)).ToArray();

                if (!topics.Any()) return;
                if (!client.IsConnected) return;

                string message = $"Subscribing to topic(s) {string.Join(", ", topics)} ";
                try {
                    var topicFilters = topics.Select(name => new MqttTopicFilter {
                        Topic = name,
                        QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                    });
                    await client.SubscribeAsync(topicFilters.ToArray());
                    logger.LogDebug(message + "ok.");

                } catch (Exception e) {
                    logger.LogError(e, message + "failed.");
                }
            }
        }

        /// <summary>
        /// Remove a topic from subscription list and potentially update with the broker.
        /// </summary>
        /// <param name="topic">The topic to remove.</param>
        /// <returns>Task that completes when the communication with broker is done.</returns>
        public Task Unsubscribe(string topic) => Unsubscribe(new[] { topic });

        /// <summary>
        /// Remove topics from subscription list and potentially update with the broker.
        /// </summary>
        /// <param name="topics">List of the topics to remove.</param>
        /// <returns>Task that completes when the communication with broker is done.</returns>
        public async Task Unsubscribe(IEnumerable<string> topics) {
            using (await sem.LockAsync()) {

                topics = topics.Where(t => subscribedTopics.Remove(t)).ToArray();
                if (!topics.Any()) return;

                string message = $"Unsubscribing from topic(s) {string.Join(", ", topics.Select(t => $"'{t}'"))} ";
                try {
                    await client.UnsubscribeAsync(topics.ToArray());
                    logger.LogDebug(message + "ok.");
                } catch (Exception e) {
                    logger.LogError(e, message + "failed.");
                }
            }
        }
    }
}