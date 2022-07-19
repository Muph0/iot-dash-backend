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

    /// <summary>
    /// <para>
    /// This hosted service is responsible for managing <see cref="InterfaceEvaluator"/>s.
    /// As the database changes, it updates an internal collection of them.
    /// At all times there is exactly one <see cref="InterfaceEvaluator"/> for each interface 
    /// which returns true from <see cref="IotInterface.NeedsEvaluator()"/>.
    /// </para>
    /// </summary>
    internal sealed class HostedEvaluationService : AEntityManagerService<IotInterface, InterfaceEvaluator>, IHostedService, IServiceScope {
        private readonly MessageMediator mediator;
        private readonly MqttMediator mqtt;
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
            this.mediator = provider.GetRequiredService<MessageMediator>();
            this.mqtt = provider.GetRequiredService<MqttMediator>();
            this.guard = new();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            this.scope = factory.CreateScope();
            this.AfterManagerUp += this.HostedEvaluationService_AfterManagerUp;
        }

        private Task HostedEvaluationService_AfterManagerUp(IotInterface entity, InterfaceEvaluator manager) {
            return manager.Update();
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