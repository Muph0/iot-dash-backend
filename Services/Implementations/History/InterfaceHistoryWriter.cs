using IotDash.Data.Model;
using IotDash.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IotDash.Domain {

    /// <summary>
    /// Takes care of writing <see cref="HistoryEntry"/> to the database when the interface value gets updated.
    /// </summary>
    internal class InterfaceHistoryWriter : IDisposable {

        private readonly (Guid DeviceId, int IfaceId) ifaceKey;
        private readonly TopicValueSubscription subscription;

        public InterfaceHistoryWriter(IotInterface iface, TopicValueSubscription subscriber) {
            this.subscription = subscriber;
            ifaceKey = (iface.DeviceId, iface.Id);
            subscriber.ValueUpdated += OnValueUpdated;
        }

        public static async Task<InterfaceHistoryWriter> Create(IotInterface iface, IServiceProvider provider) {
            var mqtt = provider.GetRequiredService<IHostedMqttClient>();

            string? topicName = iface.GetAliasTopic();
            if (topicName == null) {
                topicName = iface.GetStandardTopic();
            }

            var topic = await mqtt.GetTopic(topicName);
            TopicValueSubscription subscriber = new(provider, topic);

            return new InterfaceHistoryWriter(iface, subscriber);
        }

        private async Task OnValueUpdated(TopicUpdateEventArgs args) {
            var history = args.Context.GetRequiredService<IHistoryStore>();
            Debug.Assert(args.Subscriber == subscription);
            Debug.Assert(subscription.Value != null);

            double val = (double)subscription.Value;

            var newEntry = new HistoryEntry {
                When = DateTime.Now,

                Average = val,
                Min = val,
                Max = val,

                DeviceId = ifaceKey.DeviceId,
                InterfaceId = ifaceKey.IfaceId,
            };

            await history.CreateAsync(newEntry);
        }

        public void Dispose() {
            subscription.Dispose();
        }

    }
}
