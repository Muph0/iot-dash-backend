using IotDash.Data.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using IotDash.Contracts.V1;
using System.Diagnostics;

namespace IotDash.Services {

    interface IMessageBroker {
        void Subscribe(Type messageType, IMessageSubscriber target);
        void Subscribe<TMsg>(IMessageSubscriber<TMsg> target) where TMsg : Message => Subscribe(typeof(TMsg), target);
        void Unsubscribe(Type messageType, IMessageSubscriber target);
        void Unsubscribe<TMsg>(IMessageSubscriber<TMsg> target) where TMsg : Message => Unsubscribe(typeof(TMsg), target);
        Task Send(object sender, Message msg);
    }

    abstract class Message { }
    interface IMessageSubscriber : IDisposable {
        Type MessageType { get; }
        protected IMessageBroker MessageBroker { get; set; }
        Task OnReceive(object sender, Message message);
        void OnSubscribe(IMessageBroker broker) {
            MessageBroker = broker;
        }
        void IDisposable.Dispose() {
            MessageBroker.Unsubscribe(MessageType, this);
        }
    }
    interface IMessageSubscriber<TMsg> : IMessageSubscriber where TMsg : Message {
        Type IMessageSubscriber.MessageType => typeof(TMsg);
        Task IMessageSubscriber.OnReceive(object sender, Message message) {
            Debug.Assert(message is TMsg);
            return OnReceive(sender, (TMsg)message);
        }
        Task OnReceive(object sender, TMsg message);
    }

}