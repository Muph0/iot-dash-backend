using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Domain.Mediator {

    /// <summary>
    /// Manages subscribers and mediates messages between them.
    /// </summary>
    /// <typeparam name="TChannelKey">Type of channel identifier.</typeparam>
    /// <typeparam name="TMsg">Type of message.</typeparam>
    internal sealed class SimpleMediator<TChannelKey, TMsg> : AMediator<TChannelKey, TMsg> where TChannelKey : notnull {

        private Dictionary<TChannelKey, HashSet<ITarget<TChannelKey, TMsg>>> channels = new();

        public override int TargetCount => count;
        private int count = 0;

        public override async Task Send(TChannelKey ch, object? sender, TMsg msg) {
            var channel = GetChannelCopy(ch);
            foreach (var target in channel) {
                await target.OnReceive(sender, msg);
            }
        }

        protected override void SubscribeInternal(TChannelKey msgChannel, ITarget<TChannelKey, TMsg> target) {
            var channel = getOrCreateChannel(msgChannel);

            lock (channel) {
                if (!channel.Contains(target)) {
                    channel.Add(target);
                    Interlocked.Increment(ref count);
                } else
                    throw new InvalidOperationException("Attempt to resubscribe alrady subscribed target.");
            }
        }

        public override void Unsubscribe(ISubscription<TChannelKey, TMsg> sub) {
            Debug.Assert(sub.Target != null);
            Debug.Assert(sub.Mediator == this, "Attempt to unsubscribe subscription from different mediator.");

            var channel = getChannel(sub.Channel);

            if (channel != null) lock (channel) {
                    if (channel.Contains(sub.Target)) {
                        channel.Remove(sub.Target);
                        Interlocked.Decrement(ref count);
                        return;
                    }
                }

            throw new InvalidOperationException("Attempt to remove a target that is not subscribed.");
        }

        private ICollection<ITarget<TChannelKey, TMsg>>? getChannel(TChannelKey key) {
            lock (channels) {
                if (channels.TryGetValue(key, out var channel))
                    return channel;

                return null;
            }
        }
        private ICollection<ITarget<TChannelKey, TMsg>> getOrCreateChannel(TChannelKey key) {
            lock (channels) {
                if (channels.TryGetValue(key, out var channel))
                    return channel;

                return channels[key] = new();
            }
        }

        public override IEnumerable<ITarget<TChannelKey, TMsg>> GetChannelCopy(TChannelKey key) {
            var channel = getChannel(key);
            if (channel != null) lock (channel)
                    return channel.ToImmutableArray();
            else
                return Enumerable.Empty<ITarget<TChannelKey, TMsg>>();
        }

        public override IEnumerable<TChannelKey> Keys {
            get {
                lock (channels) {
                    return channels.Keys.ToImmutableArray();
                }
            }
        }
        public override int TargetCountOnChannel(TChannelKey key) {
            var channel = getChannel(key);
            if (channel != null) lock (channel)
                    return channel.Count;
            else
                return 0;
        }
    }

}