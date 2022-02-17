using IotDash.Data.Model;
using IotDash.Domain.Events;
using IotDash.Domain.Mediator;
using IotDash.Services.Domain;
using IotDash.Services.Messaging;
using IotDash.Services.Mqtt;
using IotDash.Utils.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services.Evaluation {

    internal sealed class HostedEvaluationService : AEntityManagerService<IotInterface, InterfaceEvaluator>, IHostedService, IServiceScope {
        private readonly IMessageMediator mediator;
        private readonly AMqttMediator mqtt;
        private readonly SubscriptionGuard guard;
        private IServiceScope scope;

        public override InterfaceEvaluator ManagerFactory(IotInterface entry)
            => new InterfaceEvaluator(entry, this);
        public override object GetKey(IotInterface entity) => entity.Id;
        public override bool NeedsManager(IotInterface entry) => entry.NeedsEvaluator();
        public IServiceProvider ServiceProvider => scope.ServiceProvider;

        public HostedEvaluationService(IServiceProvider provider) : base(
            provider.GetRequiredService<ILogger<HostedEvaluationService>>()
        ) {
            this.mediator = provider.GetRequiredService<IMessageMediator>();
            this.mqtt = provider.GetRequiredService<AMqttMediator>();
            this.guard = new();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            this.scope = factory.CreateScope();
            this.AfterManagerUp += this.HostedEvaluationService_AfterManagerUp;
        }

        private Task HostedEvaluationService_AfterManagerUp(IotInterface entity, InterfaceEvaluator manager) {
            manager.Evaluate();
            return Task.CompletedTask;
        }

        public override Task OnReceive(object? sender, SaveChangesEventArgs<IotInterface> msg) {
            logger.LogTrace($"Received {msg}.");

            return Refresh(msg.Entries
                .Where(e => e.Property(nameof(IotInterface.Expression)).IsModified
                         || e.Property(nameof(IotInterface.Kind)).IsModified)
                .Select(e => e.Entity)
            );
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            logger.LogDebug("Service starting.");
            mediator.Subscribe(this, guard);

            var db = this.scope.ServiceProvider.GetRequiredService<IInterfaceStore>();
            await Refresh(await db.GetAllAsync());
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            logger.LogDebug("Service stopping.");
            Dispose();
            return Task.CompletedTask;
        }



        protected override void Dispose(bool disposing) {
            if (DisposedValue && disposing) {
                guard.Dispose();
                this.scope.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}