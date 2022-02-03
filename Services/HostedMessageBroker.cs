using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IotDash.Services.Messages {


    class HostedMessageBroker : IMessageBroker {

        Dictionary<Type, HashSet<IMessageSubscriber>> subscribers = new();

        public async Task Send(object sender, Message msg) {
            Type? msgType = msg.GetType();

            do {
                if (subscribers.TryGetValue(msgType, out var set)) {
                    foreach (IMessageSubscriber sub in set) {
                        await sub.OnReceive(sender, msg);
                    }
                }

                msgType = msgType.BaseType;
            } while (msgType != null);
        }

        public void Subscribe(Type msgType, IMessageSubscriber target) {
            if (!subscribers.ContainsKey(msgType)) {
                subscribers[msgType] = new();
            }

            subscribers[msgType].Add(target);
        }

        public void Unsubscribe(Type msgType, IMessageSubscriber target) {
            if (subscribers.TryGetValue(msgType, out var set)) {
                set.Remove(target);
            }
        }
    }

}