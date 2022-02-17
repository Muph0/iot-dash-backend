using IotDash.Domain.Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services.Messaging.Implementation {


    internal sealed class MessageMediator : IMessageMediator {

        private ILogger logger;
        private SimpleMediator<Type, object> mediator = new();

        public MessageMediator(ILogger<MessageMediator> logger) {
            this.logger = logger;
        }

        public override int TargetCount => mediator.TargetCount;
        public override IEnumerable<Type> Keys => mediator.Keys;
        public override IEnumerable<ITarget<Type, object>> GetChannelCopy(Type key)
            => mediator.GetChannelCopy(key);
        public override int TargetCountOnChannel(Type key) => mediator.TargetCountOnChannel(key);

        public override async Task Send(Type ch, object? sender, object msg) {
            logger.LogTrace($"Sending {msg} to ->");

            Type? type = ch;
            while (type != null) {
                foreach (var target in mediator.GetChannelCopy(type)) {
                    logger.LogTrace($"    -> {target}");
                    await target.OnReceive(sender, msg);
                }
                type = type.BaseType;
            }
        }

        protected override void SubscribeInternal(Type msgChannel, ITarget<Type, object> target)
            => throw new InvalidOperationException();
        public override void Subscribe(Type msgChannel, ITarget<Type, object> target, out ISubscription subscription)
            => mediator.Subscribe(msgChannel, target, out subscription);
        public override void Subscribe(Type msgChannel, ITarget<Type, object> target, SubscriptionGuard guard)
            => mediator.Subscribe(msgChannel, target, guard);

        public override void Unsubscribe(ISubscription<Type, object> subscription)
            => mediator.Unsubscribe(subscription);

    }
}