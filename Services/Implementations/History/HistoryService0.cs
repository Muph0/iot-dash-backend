using IotDash.Data.Model;
using IotDash.Domain.Events;
using IotDash.Utils.Collections;
using IotDash.Services.Mqtt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services.History {

    /// <summary>
    /// Responsible for maintaining the <see cref="HistoryWriter0"/>s
    /// in accordance with the actual interfaces.
    /// </summary>
    internal class HistoryService0 : IHostedHistoryService {

        private readonly IServiceScopeFactory scopeFactory;
        private readonly IHostedMqttClient mqtt;
        private readonly ILogger logger;

        private readonly Dictionary<Guid, HistoryWriter0> writers = new();

        public HistoryService0(IServiceProvider provider, IHostedMqttClient mqtt) {
            this.scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            this.mqtt = mqtt;
            logger = provider.GetRequiredService<ILogger<HistoryService0>>();
        }

        #region Service_Lifetime
        public async Task StartAsync(CancellationToken cancellationToken) {
            await RebuildManagersForAllDevices();
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            writers.Values.ForEach(w => w.Dispose());
            return Task.CompletedTask;
        }
        #endregion

        #region Event_Handlers
        public async Task OnSaveChangesAsync(SaveChangesEventArgs<IotInterface> args) {

            foreach (var ifaceEntry in args.Entries) {

                var alias = ifaceEntry.Property(nameof(IotInterface.Topic));
                var logHistory = ifaceEntry.Property(nameof(IotInterface.HistoryEnabled));

                if (ifaceEntry.State == EntityState.Deleted) {
                    await DiscardManagerFor(ifaceEntry.Entity);
                } else if (alias.IsModified || logHistory.IsModified) {
                    await RebuildManagerFor(ifaceEntry.Entity, args.Scope);
                }
            }
        }

        public async Task RebuildManagersForAllDevices() {
            logger.LogDebug("Building all history writers.");
            using (var scope = scopeFactory.CreateScope()) {
                await ((IHostedInterfaceManager)this).RebuildManagers(scope.ServiceProvider);
            }
        }

        public async Task RebuildManagerFor(IotInterface iface, IServiceProvider scope) {

            await DiscardManagerFor(iface);

            if (iface.HistoryEnabled) {
                logger.LogTrace($"Creating history writer for interface {{{iface.GetTopicName()}}}.");
                var writer = await HistoryWriter0.Create(iface, scope);
                writers.Add(iface.Id, writer);
            }
        }

        public Task DiscardManagerFor(IotInterface iface) {
            if (writers.Remove(iface.Id, out var mgr)) {
                logger.LogTrace($"Discarding writer for interface {{{iface.GetTopicName()}}}");
                mgr.Dispose();
            }
            return Task.CompletedTask;
        }
        #endregion
    }

}