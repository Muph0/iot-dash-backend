using MQTTnet;
using System;
using System.Threading.Tasks;

namespace IotDash.Domain {
    interface ISubscribedTopic {
        string Name { get; }
        void Unregister(ISubscriber subscriber);
        void Register(ISubscriber subscriber);
    }

    class TopicUpdateEventArgs : EventArgs {
        public TopicUpdateEventArgs(IServiceProvider context, MqttApplicationMessageReceivedEventArgs mqttArgs, ISubscriber handler) {
            Context = context;
            MqttArgs = mqttArgs;
            Subscriber = handler;
        }

        public ISubscriber Subscriber { get; }
        public IServiceProvider Context { get; }
        public MqttApplicationMessageReceivedEventArgs MqttArgs { get; }
    }

    interface ISubscriber {
        Task OnMessageReceived(TopicUpdateEventArgs args);
    }
}
