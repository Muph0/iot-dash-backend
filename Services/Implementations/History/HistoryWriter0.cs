using IotDash.Data.Model;
using IotDash.Domain;
using IotDash.Services;
using IotDash.Services.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IotDash.Services.History {

    /// <summary>
    /// Takes care of writing <see cref="HistoryEntry"/> to the database when the interface value gets updated.
    /// </summary>
    internal class HistoryWriter0 : IDisposable {

        private readonly Guid ifaceKey;
        private readonly TopicValueSubscription subscription;
        private readonly ILogger logger;

        public HistoryWriter0(IotInterface iface, TopicValueSubscription subscriber, ILogger<HistoryWriter0> logger) {
            this.subscription = subscriber;
            ifaceKey = iface.Id;
            subscriber.ValueUpdated += OnValueUpdated;
            this.logger = logger;
        }

        public static async Task<HistoryWriter0> Create(IotInterface iface, IServiceProvider provider) {
            var mqtt = provider.GetRequiredService<IHostedMqttClient>();
            var logger = provider.GetRequiredService<ILogger<HistoryWriter0>>();

            string topicName = iface.GetTopicName();

            var topic = await mqtt.GetTopic(topicName);
            TopicValueSubscription subscription = new(provider, topic);

            return new HistoryWriter0(iface, subscription, logger);
        }

        private async Task OnValueUpdated(TopicUpdateEventArgs args) {
            var history = args.Context.GetRequiredService<IHistoryStore>();
            Debug.Assert(args.Subscriber == subscription);
            Debug.Assert(subscription.Value != null);

            double val = (double)subscription.Value;

            var newEntry = new HistoryEntry {
                WhenUTC = DateTime.UtcNow,

                Average = val,
                Min = val,
                Max = val,

                InterfaceId = ifaceKey,
            };

            logger.LogTrace($"Logging {{{subscription.Topic.Name}}}: {val}");

            await history.CreateAsync(newEntry);
            await history.SaveChangesAsync();
        }

        public void Dispose() {
            subscription.Dispose();
        }

    }
}
