
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IotDash.Data.Model;
using System.Threading.Tasks;
using System.Threading;
using System;
using IotDash.Services.Implementations.ModelStore;
using IotDash.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IotDash.Data {

    internal class DataContext : IdentityDbContext {

        public DataContext(DbContextOptions<DataContext> options)
            : base(options) {
        }

        public DbSet<IotDevice> Devices { get; set; }
        public DbSet<IotInterface> Interfaces { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<IotInterface>().HasKey(c => new { c.Id, c.DeviceId });
        }

        ICollection<IModelTracker> modelTrackers = new HashSet<IModelTracker>();
        public void RegisterTracker<TEntity>(IModelTracker<TEntity> tracker) where TEntity : class {
            modelTrackers.Add(tracker);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {

            foreach (var tracker in modelTrackers) {
                var modified = ChangeTracker.Entries()
                    .Where(entry => entry.Metadata.ClrType == tracker.EntityType && entry.State != EntityState.Unchanged)
                    .ToList();
                await tracker.OnSaveChangesAsync(modified);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }

}
