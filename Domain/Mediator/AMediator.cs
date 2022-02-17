using IotDash.Data.Model;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using IotDash.Contracts.V1;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace IotDash.Domain.Mediator {


    abstract class AMediator<TChannelKey, TMsg> where TChannelKey : notnull where TMsg : notnull {
        public virtual void Subscribe(TChannelKey msgChannel, ITarget<TChannelKey, TMsg> target, out ISubscription subscription) {
            SubscribeInternal(msgChannel, target);
            var sub = new Subscription(msgChannel, this, target);
            subscription = sub;
        }
        public virtual void Subscribe(TChannelKey msgChannel, ITarget<TChannelKey, TMsg> target, SubscriptionGuard guard) {
            SubscribeInternal(msgChannel, target);
            var sub = new Subscription(msgChannel, this, target);
            guard.Add(sub);
        }
        public abstract int TargetCount { get; }
        public abstract int TargetCountOnChannel(TChannelKey key);
        public abstract IEnumerable<TChannelKey> Keys { get; }
        public abstract IEnumerable<ITarget<TChannelKey, TMsg>> GetChannelCopy(TChannelKey key);

        protected abstract void SubscribeInternal(TChannelKey msgChannel, ITarget<TChannelKey, TMsg> target);
        public abstract void Unsubscribe(ISubscription<TChannelKey, TMsg> subscription);

        /// <summary>
        /// Send a message to all targets on the channel.
        /// </summary>
        /// <param name="msgChannel">The channel to send on.</param>
        /// <param name="sender">The message sender (optional).</param>
        /// <param name="msg">Message to send.</param>
        /// <returns></returns>
        public abstract Task Send(TChannelKey msgChannel, object sender, TMsg msg);

        private sealed class Subscription : ISubscription<TChannelKey, TMsg> {
            public TChannelKey Channel { get; }
            public AMediator<TChannelKey, TMsg>? Mediator { get; private set; }
            public ITarget<TChannelKey, TMsg>? Target { get; private set; }

            public Subscription(TChannelKey channel, AMediator<TChannelKey, TMsg> broker, ITarget<TChannelKey, TMsg> target) {
                Channel = channel;
                Mediator = broker;
                Target = target;
            }

            public void Dispose() {
                if (Mediator != null && Target != null) {
                    Mediator.Unsubscribe(this);
                    Target = null;
                    Mediator = null;
                }
            }
        }
    }

    interface IMessageTarget<in TMsg> : ITarget<Type, TMsg> where TMsg : notnull {
        //Type ITarget<Type, TMsg>.Channel => typeof(TMsg);
    }



}