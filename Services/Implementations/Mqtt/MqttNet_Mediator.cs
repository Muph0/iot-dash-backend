using IotDash.Domain.Mediator;
using IotDash.Services.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace IotDash.Services.Mqtt.Implementation {


    /// <summary>
    /// A component of the <see cref="HostedMqttService"/>.
    /// Specialisation of <see cref="MqttMediator"/> designed to work with the <see cref="MQTTnet.Client.MqttClient"/>.
    /// </summary>
    internal sealed class MqttNet_Mediator : MqttMediator {

        private readonly SimpleMediator<string, MqttApplicationMessage> mediator = new();
        private readonly HostedMqttService serviceRoot;
        private readonly MessageMediator messageMediator;
        private readonly MqttNet_Publisher publisher;
        private readonly MqttNet_Subscriber subscriber;
        private readonly ILogger<MqttMediator> logger;

        public MqttNet_Mediator(HostedMqttService serviceRoot, IServiceProvider provider) {
            this.publisher = serviceRoot.Publisher;
            this.subscriber = serviceRoot.Subscriber;
            this.logger = provider.GetRequiredService<ILogger<MqttMediator>>();
            this.serviceRoot = serviceRoot;
            this.messageMediator = provider.GetRequiredService<MessageMediator>();
        }

        public override int TargetCount => mediator.TargetCount;
        public override IEnumerable<string> Keys => mediator.Keys;
        public override int TargetCountOnChannel(string key) => mediator.TargetCountOnChannel(key);
        public override bool HasSubscribersOnTopic(string topic)
            => mediator.GetChannelCopy(topic).Count() > 0;
        public override IEnumerable<ITarget<string, MqttApplicationMessage>> GetChannelCopy(string key)
            => mediator.GetChannelCopy(key);

        /// <summary>
        /// Send or receive a message. 
        /// If the sender is the associated <see cref="HostedMqttService"/>, then the message is broadcasted to in-application listeners.
        /// Otherwise the sender is from inside the application, so the message is sent to the broker.
        /// </summary>
        /// <param name="msgChannel">The MQTT topic.</param>
        /// <param name="sender">Sender of the message.</param>
        /// <param name="msg">Message to send.</param>
        /// <returns></returns>
        protected override async Task MqttSend(string msgChannel, object sender, MqttApplicationMessage msg) {

            if (sender == serviceRoot) {
                this.logger.LogTrace($"Receiving [\"{msg.Topic}\"]: \"{msg.ConvertPayloadToString()}\"");
                await mediator.Send(msgChannel, sender, msg);
            } else {
                this.logger.LogTrace($"Sending [\"{msg.Topic}\"]: \"{msg.ConvertPayloadToString()}\"");
                await publisher.SendAsync(msg);
            }

            // produce an application-level event
            await messageMediator.Send(sender, msg);
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

        /// <summary>
        /// Subscribe <paramref name="target"/> to all messages on given MQTT <paramref name="topic"/> producing a <paramref name="subscription"/>.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="target">The target of the events.</param>
        /// <param name="subscription">Subscription object.</param>
        public override void Subscribe(string topic, ITarget<string, MqttApplicationMessage> target, out ISubscription subscription) {
            mediator.Subscribe(topic, target, out subscription);
            updateSubscriptionState(topic);
        }
        /// <summary>
        /// Subscribe <paramref name="target"/> to all messages on given MQTT <paramref name="topic"/>
        /// registering the produced subscription under a given <paramref name="guard"/>.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="target">The target of the events.</param>
        /// <param name="guard">Subscription guard under which to register produced <see cref="ISubscription"/>.</param>
        public override void Subscribe(string topic, ITarget<string, MqttApplicationMessage> target, SubscriptionGuard guard) {
            mediator.Subscribe(topic, target, guard);
            updateSubscriptionState(topic);
        }

        /// <summary>
        /// Cancel the given <paramref name="subscription"/>.
        /// </summary>
        /// <param name="subscription">The subscription to cancel.</param>
        public override void Unsubscribe(ISubscription<string, MqttApplicationMessage> subscription) { 
            mediator.Unsubscribe(subscription);
            updateSubscriptionState(subscription.Channel);
        }
    }

}