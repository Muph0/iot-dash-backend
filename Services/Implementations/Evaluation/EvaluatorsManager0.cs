using IotDash.Data;
using IotDash.Data.Model;
using IotDash.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using IotDash.Utils.Collections;
using System.Diagnostics;
using IotDash.Services.Mqtt;
using IotDash.Domain.Events;

namespace IotDash.Services.Evaluation {
    
    [Obsolete]
    internal class EvaluatorsManager0 : IHostedEvaluationService {

        private readonly IHostedMqttClient mqtt;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger logger;
        private readonly Dictionary<Guid, IManagedEvaluator> evaluators = new();

        public EvaluatorsManager0(IServiceScopeFactory scopeFactory, ILogger<EvaluatorsManager0> logger) {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken) {
            logger.LogDebug($"Rebuilding all interface managers.");
            await RebuildManagersForAllDevices();
        }


        async Task IModelTracker<IotInterface>.OnSaveChangesAsync(SaveChangesEventArgs<IotInterface> args) {

            foreach (var ifaceEntry in args.Entries) {

                var alias = ifaceEntry.Property(nameof(IotInterface.Topic));
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
                await ((IHostedInterfaceManager)this).RebuildManagers(scope.ServiceProvider);
            }
        }

        public async Task RebuildManagerFor(IotInterface iface, IServiceProvider provider) {

            await DiscardManagerFor(iface);

            if (iface.NeedsEvaluator() && iface.Expression != null) {
                Debug.Assert(iface.Expression != string.Empty);
                logger.LogTrace($"Creating evaluator for interface {{{iface.GetTopicName()}}}.");
                var evaluator = await InterfaceEvaluator0.Create(provider, iface);
                await evaluator.Evaluate(provider);
                evaluators.Add(iface.Id, evaluator);
            }
        }

        public Task DiscardManagerFor(IotInterface iface) {
            if (evaluators.Remove(iface.Id, out var mgr)) {
                logger.LogTrace($"Discarding evaluator for interface {{{iface.GetTopicName()}}}");
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