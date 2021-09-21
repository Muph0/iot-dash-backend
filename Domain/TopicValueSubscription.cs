using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace IotDash.Domain {
    class TopicValueSubscription : IDisposable, ISubscriber {

        public ISubscribedTopic Topic { get; }
        public double? Value { get; private set; }
        public double? OldValue { get; private set; }

        public bool IsValueChanged => Value != OldValue;

        private readonly ILogger logger;

        public delegate Task AsyncValueChangeHandler(TopicUpdateEventArgs args);
        public event AsyncValueChangeHandler ValueUpdated;

        public TopicValueSubscription(IServiceProvider provider, ISubscribedTopic topic) {
            this.logger = provider.GetRequiredService<ILogger<TopicValueSubscription>>();
            this.Topic = topic;
            topic.Register(this);
        }

        public async Task OnMessageReceived(TopicUpdateEventArgs eventArgs) {

            string payload = Encoding.ASCII.GetString(eventArgs.MqttArgs.ApplicationMessage.Payload);

            if (double.TryParse(payload, out double newValue)) {
                OldValue = Value;
                Value = newValue;

                var evt = ValueUpdated;
                if (evt != null) {
                    foreach (var handler in evt.GetInvocationList().Cast<AsyncValueChangeHandler>()) {
                        await handler.Invoke(eventArgs);
                    }
                }
            } else {
                logger.LogError($"Failed to parse topic value '{payload}' as a decimal number.");
            }
        }

        public void Dispose() {
            Topic?.Unregister(this);
        }

    }
}
