using System;
using System.Collections;
using System.Collections.Generic;

namespace IotDash.Domain.Mediator {

    /// <summary>
    /// <para>
    /// Represents a cancellable subscription.
    /// <br/>
    /// Call <c>Dispose()</c> to cancel the subscription.
    /// </para>
    /// </summary>
    interface ISubscription : IDisposable {
        bool IsDisposed { get; }
    }

    /// <summary>
    /// <para>
    /// Represents a cancellable subscription to a particular channel of typed messages.
    /// <br/>
    /// Call <c>Dispose()</c> to cancel the subscription.
    /// </para>
    /// </summary>
    /// <typeparam name="TChannelKey">Channel this subscription is subscribed to.</typeparam>
    /// <typeparam name="TMsg">Type of the messages being send in the channel.</typeparam>
    interface ISubscription<TChannelKey, TMsg> : ISubscription
            where TChannelKey : notnull
            where TMsg : notnull {

        /// <summary>
        /// Channel of this subscription.
        /// </summary>
        TChannelKey Channel { get; }

        /// <summary>
        /// The mediator from which this subscription comes.
        /// </summary>
        AbstractMediator<TChannelKey, TMsg>? Mediator { get; }

        /// <summary>
        /// The target to which this subscription sends events.
        /// </summary>
        ITarget<TChannelKey, TMsg>? Target { get; }

        /// <summary>
        /// If true, the subscription is cancelled.
        /// </summary>
        bool ISubscription.IsDisposed => Mediator == null;
    }

    /// <summary>
    /// Represents a collection of <see cref="ISubscription"/>s.
    /// Disposing of the <see cref="SubscriptionGuard"/> disposes of all the registered <see cref="ISubscription"/>s.
    /// </summary>
    internal sealed class SubscriptionGuard : IDisposable {
        private bool IsDisposed => subscriptions == null;
        private List<ISubscription> subscriptions = new();

        /// <summary>
        /// Register a subscription under this guard.
        /// </summary>
        /// <param name="item">The subscription to add.</param>
        public void Add(ISubscription item) {
            subscriptions.Add(item);
        }

        /// <summary>
        /// Dispose of all registered subscriptions.
        /// </summary>
        public void Dispose() {
            if (!IsDisposed) {
                foreach (var sub in subscriptions) {
                    sub.Dispose();
                }
                subscriptions.Clear();
            }

            GC.SuppressFinalize(this);
        }
    }
}