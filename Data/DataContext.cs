using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IotDash.Data.Model;
using System.Threading.Tasks;
using System.Threading;
using System;
using IotDash.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using IotDash.Services.Messaging;
using Microsoft.Extensions.DependencyInjection;
using IotDash.Domain.Events;
using Microsoft.Extensions.Logging;

namespace IotDash.Data {

    internal class DataContext : IdentityDbContext {
        static bool staticInit = true;

        private readonly IServiceProvider provider;
        private readonly MessageMediator mediator;
        private readonly ILogger<DataContext> logger;

        public DataContext(DbContextOptions<DataContext> options, IServiceProvider provider)
            : base(options) {
            this.provider = provider;
            this.mediator = provider.GetRequiredService<MessageMediator>();
            this.logger = provider.GetRequiredService<ILogger<DataContext>>();

            if (staticInit) {
                staticInit = false;
                logger.LogInformation("Initialising database.");
            }
        }

        public DbSet<IotInterface> Interfaces { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<HistoryEntry> History { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<HistoryEntry>().HasIndex(e => e.InterfaceId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {

            // new way
            var entryGroups = ChangeTracker.Entries()
                .Where(entry => entry.State != EntityState.Unchanged)
                .GroupBy(entry => entry.Metadata.ClrType);

            foreach (var group in entryGroups) {
                await this.mediator.SendSaveChangesEventArgs(group.Key, this, group, provider);
            }

            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }
    }

}
