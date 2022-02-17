using IotDash.Domain.Mediator;
using MQTTnet;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IotDash.Services.Mqtt {
    abstract class AMqttMediator : AMediator<string, MqttApplicationMessage> {
        public abstract bool HasSubscribersOnTopic(string topic);

        private Dictionary<string, MqttApplicationMessage> retained = new();

        /// <summary>
        /// Return last message, null if there was no mesagge.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public MqttApplicationMessage? GetRetained(string topic) {
            if (retained.TryGetValue(topic, out var msg)) {
                return msg;
            }
            return null;
        }

        protected abstract Task MqttSend(string channel, object sender, MqttApplicationMessage msg);

        /// <summary>
        /// Sends the provided application messages.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override Task Send(string channel, object sender, MqttApplicationMessage msg) {
            retained[channel] = msg;
            return MqttSend(channel, sender, msg);
        }

        /// <summary>
        /// Builds a new application message with default parameters and broadcasts it.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public Task Send(string topic, object sender, string content) {
            var msg = new MqttApplicationMessageBuilder()
                .WithAtLeastOnceQoS()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(content))
                .Build();

            return Send(topic, sender, msg);
        }

    }

    interface IMqttSubscriber : ITarget<string, MqttApplicationMessage> { }
}
