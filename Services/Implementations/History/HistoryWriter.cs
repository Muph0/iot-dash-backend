using IotDash.Data;
using IotDash.Data.Model;
using IotDash.Domain.Mediator;
using IotDash.Utils.Debugging;
using IotDash.Services.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System;
using System.Threading.Tasks;
using IotDash.Controllers.V1;
using Microsoft.AspNetCore.SignalR;

namespace IotDash.Services.History {

    internal class HistoryWriter : IDisposable, IMqttSubscriber {
        private readonly SubscriptionGuard guard = new();
        private readonly IotInterface entity;
        private readonly IHistoryStore store;
        private readonly ILogger logger;
        private readonly IHubContext<ChartHub> chartHub;

        public HistoryWriter(IotInterface entity, IServiceScope scope) {
            var provider = scope.ServiceProvider;
            this.entity = entity;
            this.store = provider.GetRequiredService<IHistoryStore>();
            this.logger = provider.GetRequiredService<ILogger<HistoryWriter>>();
            var mqtt = provider.GetRequiredService<MqttMediator>();
            mqtt.Subscribe(entity.GetTopicName(), this, guard);
            this.chartHub = provider.GetRequiredService<IHubContext<ChartHub>>();
        }

        async Task ITarget<string, MqttApplicationMessage>.OnReceive(object? sender, MqttApplicationMessage msg) {

            if (!double.TryParse(msg.ConvertPayloadToString(), out var val)) {
                logger.LogError($"Failed to parse {msg.ToDebugString()} as double.");
                return;
            }

            logger.LogTrace($"Writing {msg.ToDebugString()}");

            var entry = new HistoryEntry {
                WhenUTC = DateTime.UtcNow,
                Average = val,
                Min = val,
                Max = val,
                InterfaceId = entity.Id,
            };
            await store.CreateAsync(entry);
            await store.SaveChangesAsync();

            await chartHub.Clients.All.SendAsync(ChartHub.MethodNewData, new Contracts.V1.Model.HistoryEntry(entry));
        }

        public void Dispose() {
            guard.Dispose();
        }
    }
}
