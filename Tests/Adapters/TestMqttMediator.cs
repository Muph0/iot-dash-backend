using IotDash.Domain.Mediator;
using IotDash.Services.Mqtt;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Adapters {
    internal sealed class TestMqttMediator : MqttMediator {
        private SimpleMediator<string, MqttApplicationMessage> smediator = new();
        private readonly Queue<(object sender, MqttApplicationMessage msg)> sentMessages;

        public TestMqttMediator() { }
        public TestMqttMediator(Queue<(object sender, MqttApplicationMessage msg)> sentMessages) {
            this.sentMessages = sentMessages;
        }

        public override int TargetCount => smediator.TargetCount;
        public override IEnumerable<string> Keys => smediator.Keys;
        public override IEnumerable<ITarget<string, MqttApplicationMessage>> GetChannelCopy(string key) => smediator.GetChannelCopy(key);
        public override bool HasSubscribersOnTopic(string topic) => GetChannelCopy(topic).Any();
        public override int TargetCountOnChannel(string key) => GetChannelCopy(key).Count();
        public override void Unsubscribe(ISubscription<string, MqttApplicationMessage> subscription) => smediator.Unsubscribe(subscription);
        protected override async Task MqttSend(string channel, object sender, MqttApplicationMessage msg) {
            sentMessages.Enqueue((sender, msg));
            await smediator.Send(channel, sender, msg);
        }
        public override void Subscribe(string msgChannel, ITarget<string, MqttApplicationMessage> target, out ISubscription subscription)
            => smediator.Subscribe(msgChannel, target, out subscription);
        protected override void SubscribeInternal(string msgChannel, ITarget<string, MqttApplicationMessage> target) {
            throw new NotSupportedException();
        }
    }
}
