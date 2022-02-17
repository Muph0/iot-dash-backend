using System;
using System.Threading.Tasks;

namespace IotDash.Domain.Mediator {

    interface ITarget<TChannelKey, in TMsg>
            where TChannelKey : notnull
            where TMsg : notnull {
        //TChannelKey Channel { get; }
        Task OnReceive(object? sender, TMsg message);
    }

    /// <summary>
    /// Shim inserted before a subscription target, that casts all messages from TMsgIn to TMsgOut.
    /// </summary>
    /// <typeparam name="TChannelKey"></typeparam>
    /// <typeparam name="TMsgIn"></typeparam>
    /// <typeparam name="TMsgOut"></typeparam>
    internal class TargetCastAdapter<TChannelKey, TMsgIn, TMsgOut> : ITarget<TChannelKey, TMsgIn>
            where TChannelKey : notnull
            where TMsgIn : notnull
            where TMsgOut : notnull, TMsgIn {
        //public TChannelKey Channel => target.Channel;
        private readonly ITarget<TChannelKey, TMsgOut> target;

        public TargetCastAdapter(ITarget<TChannelKey, TMsgOut> target) {
            this.target = target;
        }

        public Task OnReceive(object? sender, TMsgIn message) {
            return target.OnReceive(sender, (TMsgOut)message);
        }

        public override string ToString() {
            return $"{nameof(TargetCastAdapter<TChannelKey, TMsgIn, TMsgOut>)}({target})";
        }
    }
}