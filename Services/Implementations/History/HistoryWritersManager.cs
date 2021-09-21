using IotDash.Data.Model;
using IotDash.Domain;
using IotDash.Extensions.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services {

    /// <summary>
    /// Responsible for maintaining the <see cref="InterfaceHistoryWriter"/>s
    /// in accordance with the actual interfaces.
    /// </summary>
    internal class HistoryWritersManager : IHostedHistoryService {

        private readonly IServiceScopeFactory scopeFactory;
        private readonly IHostedMqttClient mqtt;
        private readonly ILogger logger;

        private readonly Dictionary<(Guid, int), InterfaceHistoryWriter> writers = new();

        public HistoryWritersManager(IServiceProvider provider, IHostedMqttClient mqtt) {
            this.scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            this.mqtt = mqtt;
            logger = provider.GetRequiredService<ILogger<HistoryWritersManager>>();
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

            foreach (var ifaceEntry in args.Changed) {

                var alias = ifaceEntry.Property(nameof(IotInterface.Alias));
                var logHistory = ifaceEntry.Property(nameof(IotInterface.LogHistory));

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
                await ((IHostedInterfaceManager)this).RebuildManagersForAllDevices(scope.ServiceProvider);
            }
        }

        public async Task RebuildManagerFor(IotInterface iface, IServiceProvider scope) {

            await DiscardManagerFor(iface);

            if (iface.LogHistory) {
                logger.LogTrace($"Creating history writer for interface {{{iface.GetStandardTopic()}}}.");
                var writer = await InterfaceHistoryWriter.Create(iface, scope);
                writers.Add(iface.GetKey(), writer);
            }
        }

        public Task DiscardManagerFor(IotInterface iface) {
            if (writers.Remove(iface.GetKey(), out var mgr)) {
                logger.LogTrace($"Discarding writer for interface {{{iface.GetStandardTopic()}}}");
                mgr.Dispose();
            }
            return Task.CompletedTask;
        }
        #endregion
    }

}