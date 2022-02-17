using System.Threading.Tasks;
using System;
using IotDash.Domain.Mediator;

namespace IotDash.Services.Messaging {
    abstract class IMessageMediator : AMediator<Type, object> {
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