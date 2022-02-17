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
        private readonly IMessageMediator mediator;
        private readonly ILogger<DataContext> logger;
        ICollection<IModelTracker> modelTrackers = new HashSet<IModelTracker>(8);

        public DataContext(DbContextOptions<DataContext> options, IServiceProvider provider)
            : base(options) {
            this.provider = provider;
            this.mediator = provider.GetRequiredService<IMessageMediator>();
            this.logger = provider.GetRequiredService<ILogger<DataContext>>();

            if (staticInit) {
                staticInit = false;
                logger.LogInformation("Initialising database.");
            }

            //RegisterTracker(eval);
            //RegisterTracker(history);
        }

        public DbSet<IotInterface> Interfaces { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<HistoryEntry> History { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<HistoryEntry>().HasIndex(e => e.InterfaceId);
        }

        public void RegisterTracker<TEntity>(IModelTracker<TEntity> tracker) where TEntity : class {
            modelTrackers.Add(tracker);
        }

        bool saving = false;
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {

            if (!saving) {
                //saving = true;

                // old way
                //foreach (var tracker in modelTrackers) {
                //    var modified = ChangeTracker.Entries()
                //        .Where(entry => entry.Metadata.ClrType == tracker.EntityType && entry.State != EntityState.Unchanged)
                //        .ToList();
                //    var evt = new Domain.Events.SaveChangesEventArgs(modified, provider);
                //    await tracker.OnSaveChangesAsync(evt);
                //}

                // new way
                var entryGroups = ChangeTracker.Entries()
                    .Where(entry => entry.State != EntityState.Unchanged)
                    .GroupBy(entry => entry.Metadata.ClrType);

                foreach (var group in entryGroups) {
                    await this.mediator.SendSaveChangesEventArgs(group.Key, this, group, provider);
                }

                //saving = false;
            }

            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }
    }

}
