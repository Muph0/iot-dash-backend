using IotDash.Data.Model;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using IotDash.Contracts.V1;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace IotDash.Domain.Mediator {

    /// <summary>
    /// This class implements part the mediator design pattern.
    /// Takes responsibility for managing subscriptions of channels.
    /// The implementation of the actual message delegation is left to the child class of this.
    /// </summary>
    /// <typeparam name="TChannelKey">Type of the channel identifiers.</typeparam>
    /// <typeparam name="TMsg">Type of the messages which are delegated by this mediator.</typeparam>
    abstract class AbstractMediator<TChannelKey, TMsg> where TChannelKey : notnull where TMsg : notnull {

        /// <summary>
        /// Register a new message target to receive messages from a particular channel. 
        /// Creates the <see cref="ISubscription"/> and calls <see cref="SubscribeInternal(TChannelKey, ITarget{TChannelKey, TMsg})"/>.
        /// </summary>
        /// <param name="msgChannel">The channel identifier.</param>
        /// <param name="target">The message target.</param>
        /// <param name="subscription">Subscription object. Dispose it to cancel the subscription.</param>
        public virtual void Subscribe(TChannelKey msgChannel, ITarget<TChannelKey, TMsg> target, out ISubscription subscription) {
            SubscribeInternal(msgChannel, target);
            var sub = new Subscription(msgChannel, this, target);
            subscription = sub;
        }

        /// <summary>
        /// Register a new message target to receive messages from a particular channel.
        /// Registers the <see cref="ISubscription"/> under the given <paramref name="guard"/> and calls <see cref="SubscribeInternal(TChannelKey, ITarget{TChannelKey, TMsg})"/>.
        /// </summary>
        /// <param name="msgChannel">The channel identifier.</param>
        /// <param name="target">The message target.</param>
        /// <param name="guard">Subscription guard to manage the created subscription.</param>
        public virtual void Subscribe(TChannelKey msgChannel, ITarget<TChannelKey, TMsg> target, SubscriptionGuard guard) {
            SubscribeInternal(msgChannel, target);
            var sub = new Subscription(msgChannel, this, target);
            guard.Add(sub);
        }

        /// <summary>
        /// Number of <see cref="ITarget{TChannelKey, TMsg}"/> subscribed to this mediator.
        /// </summary>
        public abstract int TargetCount { get; }

        /// <summary>
        /// Number of <see cref="ITarget{TChannelKey, TMsg}"/> subscribed to channel <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The channel to filter by.</param>
        public abstract int TargetCountOnChannel(TChannelKey key);

        /// <summary>
        /// <see cref="IEnumerable{TChannelKey}"/> of the channels on this mediator.
        /// </summary>
        public abstract IEnumerable<TChannelKey> Keys { get; }

        /// <summary>
        /// A list of all subscribers of channel <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key identifying that channel.</param>
        public abstract IEnumerable<ITarget<TChannelKey, TMsg>> GetChannelCopy(TChannelKey key);

        /// <summary>
        /// Internal, <see cref="ISubscription"/> unaware method prepared for inheritor implementation.
        /// It is here to prevent the inheritor from forgetting to properly create the <see cref="ISubscription"/>.
        /// </summary>
        /// <param name="msgChannel"></param>
        /// <param name="target"></param>
        protected abstract void SubscribeInternal(TChannelKey msgChannel, ITarget<TChannelKey, TMsg> target);
        public abstract void Unsubscribe(ISubscription<TChannelKey, TMsg> subscription);

        /// <summary>
        /// Send a message to a channel.
        /// </summary>
        /// <param name="msgChannel">The channel to send over.</param>
        /// <param name="sender">The message sender (optional).</param>
        /// <param name="msg">Message to send.</param>
        /// <returns></returns>
        public abstract Task Send(TChannelKey msgChannel, object sender, TMsg msg);

        /// <summary>
        /// Implementation of <see cref="ISubscription{TChannelKey, TMsg}"/> suitable for this <see cref="AbstractMediator{TChannelKey, TMsg}"/>.
        /// </summary>
        private sealed class Subscription : ISubscription<TChannelKey, TMsg> {
            public TChannelKey Channel { get; }
            public AbstractMediator<TChannelKey, TMsg>? Mediator { get; private set; }
            public ITarget<TChannelKey, TMsg>? Target { get; private set; }

            public Subscription(TChannelKey channel, AbstractMediator<TChannelKey, TMsg> broker, ITarget<TChannelKey, TMsg> target) {
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

    /// <summary>
    /// Specialized <see cref="ITarget{TChannelKey, TMsg}"/>, where the channel key is the <see cref="Type"/> of the message.
    /// </summary>
    /// <typeparam name="TMsg"></typeparam>
    interface IMessageTarget<in TMsg> : ITarget<Type, TMsg> where TMsg : notnull {
        //Type ITarget<Type, TMsg>.Channel => typeof(TMsg);
    }



}