using System.Threading.Tasks;
using System;
using IotDash.Domain.Mediator;

namespace IotDash.Services.Messaging {

    /// <summary>
    /// This class is a specialisation of the <see cref="AbstractMediator{TChannelKey, TMsg}"/>
    /// which mediates objects over channels differentiated by the objects' types.
    /// It provides nicer API than the parent class.
    /// </summary>
    abstract class MessageMediator : AbstractMediator<Type, object> {

        /// <summary>
        /// Subscribe <paramref name="target"/> for message of type <typeparamref name="TMsg"/>.
        /// Produced <paramref name="subscription"/> is used to cancel the subscription.
        /// </summary>
        /// <typeparam name="TMsg"></typeparam>
        /// <param name="target"></param>
        /// <param name="subscription"></param>
        public void Subscribe<TMsg>(ITarget<Type, TMsg> target, out ISubscription subscription) where TMsg : notnull {
            var adapter = new TargetCastAdapter<Type, object, TMsg>(target);
            Subscribe(typeof(TMsg), adapter, out subscription);
        }
        public void Subscribe<TMsg>(ITarget<Type, TMsg> target, SubscriptionGuard guard) where TMsg : notnull {
            var adapter = new TargetCastAdapter<Type, object, TMsg>(target);
            Subscribe(typeof(TMsg), adapter, guard);
        }
        public Task Send<TMsg>(object? sender, TMsg message) where TMsg : notnull {
            return Send(typeof(TMsg), sender, message);
        }
    }



}