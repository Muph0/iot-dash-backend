using IotDash.Domain.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace IotDash.Services.Mqtt.Implementation {



    internal sealed class MqttNet_Mediator : AMqttMediator {

        private readonly SimpleMediator<string, MqttApplicationMessage> mediator = new();
        private readonly HostedMqttService serviceRoot;
        private readonly MqttNet_Publisher publisher;
        private readonly MqttNet_Subscriber subscriber;
        private readonly ILogger<AMqttMediator> logger;

        public MqttNet_Mediator(HostedMqttService serviceRoot, IServiceProvider provider) {
            this.publisher = serviceRoot.Publisher;
            this.subscriber = serviceRoot.Subscriber;
            this.logger = provider.GetRequiredService<ILogger<AMqttMediator>>();
            this.serviceRoot = serviceRoot;
        }

        public override int TargetCount => mediator.TargetCount;
        public override IEnumerable<string> Keys => mediator.Keys;
        public override int TargetCountOnChannel(string key) => mediator.TargetCountOnChannel(key);
        public override bool HasSubscribersOnTopic(string topic)
            => mediator.GetChannelCopy(topic).Count() > 0;
        public override IEnumerable<ITarget<string, MqttApplicationMessage>> GetChannelCopy(string key)
            => mediator.GetChannelCopy(key);

        protected override async Task MqttSend(string msgChannel, object sender, MqttApplicationMessage msg) {

            if (sender == serviceRoot) {

                this.logger.LogTrace($"Receiving [\"{msg.Topic}\"]: \"{msg.ConvertPayloadToString()}\"");
                await mediator.Send(msgChannel, sender, msg);
            } else {

                this.logger.LogTrace($"Sending [\"{msg.Topic}\"]: \"{msg.ConvertPayloadToString()}\"");
                await publisher.SendAsync(msg);
            }
        }

        void updateSubscriptionState(string topic) {
            Task fireAndForget;
            if (mediator.TargetCountOnChannel(topic) > 0) {
                fireAndForget = subscriber.Subscribe(topic);
            } else {
                fireAndForget = subscriber.Unsubscribe(topic);
            }
            serviceRoot.Unwrapper.Unwrap(fireAndForget);
        }

        protected override void SubscribeInternal(string topic, ITarget<string, MqttApplicationMessage> target) {
            throw new InvalidOperationException();
        }
        public override void Subscribe(string topic, ITarget<string, MqttApplicationMessage> target, out ISubscription subscription) {
            mediator.Subscribe(topic, target, out subscription);
            updateSubscriptionState(topic);
        }
        public override void Subscribe(string topic, ITarget<string, MqttApplicationMessage> target, SubscriptionGuard guard) {
            mediator.Subscribe(topic, target, guard);
            updateSubscriptionState(topic);
        }
        public override void Unsubscribe(ISubscription<string, MqttApplicationMessage> subscription) { 
            mediator.Unsubscribe(subscription);
            updateSubscriptionState(subscription.Channel);
        }
    }

}