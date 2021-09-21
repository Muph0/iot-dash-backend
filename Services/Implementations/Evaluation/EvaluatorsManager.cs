using IotDash.Data;
using IotDash.Data.Model;
using IotDash.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using IotDash.Extensions.Collections;
using System.Diagnostics;

namespace IotDash.Services {

    internal class EvaluatorsManager : IHostedEvaluationService {

        private readonly IHostedMqttClient mqtt;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger logger;
        private readonly Dictionary<(Guid, int), IManagedEvaluator> evaluators = new();

        public EvaluatorsManager(IServiceScopeFactory scopeFactory, ILogger<EvaluatorsManager> logger) {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken) {
            logger.LogDebug($"Rebuilding all interface managers.");
            await RebuildManagersForAllDevices();
        }


        public async Task OnSaveChangesAsync(SaveChangesEventArgs<IotInterface> args) {

            foreach (var ifaceEntry in args.Changed) {

                var alias = ifaceEntry.Property(nameof(IotInterface.Alias));
                var expression = ifaceEntry.Property(nameof(IotInterface.Expression));

                if (ifaceEntry.State == EntityState.Deleted) {
                    await DiscardManagerFor(ifaceEntry.Entity);
                } else if (expression.IsModified || alias.IsModified) {
                    await RebuildManagerFor(ifaceEntry.Entity, args.Scope);
                }
            }
        }

        public async Task RebuildManagersForAllDevices() {
            using (var scope = scopeFactory.CreateScope()) {
                await ((IHostedInterfaceManager)this).RebuildManagersForAllDevices(scope.ServiceProvider);
            }
        }

        public async Task RebuildManagerFor(IotInterface iface, IServiceProvider provider) {

            await DiscardManagerFor(iface);

            if (iface.Expression != null) {
                Debug.Assert(iface.Expression != string.Empty);
                logger.LogTrace($"Creating evaluator for interface {{{iface.GetStandardTopic()}}}.");
                var evaluator = await InterfaceEvaluator.Create(provider, iface);
                await evaluator.Evaluate(provider);
                evaluators.Add(iface.GetKey(), evaluator);
            }
        }

        public Task DiscardManagerFor(IotInterface iface) {
            if (evaluators.Remove(iface.GetKey(), out var mgr)) {
                logger.LogTrace($"Discarding evaluator for interface {{{iface.GetStandardTopic()}}}");
                mgr.Dispose();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            evaluators.Values.ForEach(e => e.Dispose());
            return Task.CompletedTask;
        }

    }

}