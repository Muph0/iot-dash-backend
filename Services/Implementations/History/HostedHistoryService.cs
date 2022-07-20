
using IotDash.Data.Model;
using IotDash.Domain.Events;
using IotDash.Domain.Mediator;
using IotDash.Services.Domain;
using IotDash.Services.Messaging;
using IotDash.Services.Mqtt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services.History {

    /// <summary>
    /// <para>
    /// This hosted service is responsible for managing <see cref="HistoryWriter"/>s.
    /// As the database changes, it updates an internal collection of them.
    /// At all times there is exactly one <see cref="HistoryWriter"/> for each interface 
    /// which returns true from <see cref="IotInterface.NeedsWriter()"/>.
    /// </para>
    /// </summary>
    internal sealed class HostedHistoryService : AEntityManagerService<IotInterface, HistoryWriter>, IHostedService, IServiceScope {

        private readonly MessageMediator mediator;
        private readonly SubscriptionGuard guard;        

        public IServiceProvider ServiceProvider => scope.ServiceProvider;
        private readonly IServiceScope scope;
        public override HistoryWriter ManagerFactory(IotInterface entry) 
            => new HistoryWriter(entry, this);
        public override bool NeedsManager(IotInterface entry) 
            => entry.NeedsWriter();
        public override object GetKey(IotInterface entity) => entity.Id;

        public HostedHistoryService(IServiceProvider provider) : base(
                provider.GetRequiredService<ILogger<HostedHistoryService>>()
            ) {
            this.mediator = provider.GetRequiredService<MessageMediator>();
            this.guard = new();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            this.scope = factory.CreateScope();
        }

        public override Task OnReceive(object? sender, SaveChangesEventArgs<IotInterface> msg) {
            logger.LogTrace($"Received {msg}.");

            return Refresh(msg.Entries
                .Where(e => e.Property(nameof(IotInterface.Topic)).IsModified
                         || e.Property(nameof(IotInterface.HistoryEnabled)).IsModified)
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