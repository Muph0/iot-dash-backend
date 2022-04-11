using System;
using System.Collections;
using System.Collections.Generic;

namespace IotDash.Domain.Mediator {
    interface ISubscription : IDisposable {
        bool IsDisposed { get; }
    }

    interface ISubscription<TChannelKey, TMsg> : ISubscription
            where TChannelKey : notnull
            where TMsg : notnull {
        TChannelKey Channel { get; }
        AbstractMediator<TChannelKey, TMsg>? Mediator { get; }
        ITarget<TChannelKey, TMsg>? Target { get; }
        bool ISubscription.IsDisposed => Mediator == null;
    }

    internal sealed class SubscriptionGuard : IDisposable {
        private bool IsDisposed => subscriptions == null;
        private List<ISubscription> subscriptions = new();

        public void Add(ISubscription item) {
            subscriptions.Add(item);
        }


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