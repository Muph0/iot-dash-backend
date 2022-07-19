using IotDash.Domain.Mediator;
using MQTTnet;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IotDash.Services.Mqtt {

    /// <summary>
    /// Specialisation of the <see cref="AbstractMediator{TChannelKey, TMsg}"/> where the channel 
    /// key is of type <see cref="string"/> and the message type is <see cref="MqttApplicationMessage"/>.
    /// </summary>
    abstract class MqttMediator : AbstractMediator<string, MqttApplicationMessage> {

        /// <summary>
        /// Check if there are any subscribers on given <paramref name="topic"/>.
        /// </summary>
        /// <param name="topic">The topic to check.</param>
        /// <returns><c>true</c> if there are any subscribers.</returns>
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
        /// <param name="topic">MQTT topic of the message.</param>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="content">Payload of the message.</param>
        /// <returns>Task which completes when the message is sent.</returns>
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
