using IotDash.Data;
using IotDash.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Services.Implementations.ModelStore {

    internal class InterfaceEntityStore : IInterfaceStore {

        private readonly DataContext db;
        private readonly ILogger logger;
        private readonly IHostedExpressionManager expressions;
        private readonly IServiceProvider provider;

        public InterfaceEntityStore(DataContext db, ILogger<InterfaceEntityStore> logger, IHostedExpressionManager expressionManager, IServiceProvider provider) {
            this.db = db;
            this.logger = logger;
            this.expressions = expressionManager;
            this.provider = provider;
            db.RegisterTracker(this);
        }

        public async Task CreateAsync(IotInterface ifaceToCreate) {
            await db.Interfaces.AddAsync(ifaceToCreate);
        }

        public async Task<bool> DeleteByKeyAsync((Guid, int) key) {
            var iface = await GetByKeyAsync(key);
            if (iface == null) {
                return false;
            }
            await db.Interfaces.SingleDeleteAsync(iface);
            return true;
        }

        public async Task<IReadOnlyList<IotInterface>> GetAllAsync() {
            return await db.Interfaces.ToListAsync();
        }

        public async Task<IReadOnlyList<IotInterface>> GetAllByDeviceAsync(Guid deviceId) {
            return await db.Interfaces.Where(i => i.DeviceId == deviceId).ToListAsync();
        }

        public async Task<IotInterface?> GetByKeyAsync((Guid, int) key) {
            var (deviceId, Id) = key;
            return await db.Interfaces.SingleOrDefaultAsync(i => i.DeviceId == deviceId && i.Id == Id);
        }

        public async Task<bool> SaveChangesAsync() {
            int affected = await db.SaveChangesAsync();
            return affected > 0;
        }

        public Task OnSaveChangesAsync(IEnumerable<EntityEntry<IotInterface>> changed) {
            
            foreach (var ifaceEntry in changed) {

                var alias = ifaceEntry.Property(nameof(IotInterface.Alias));
                var expression = ifaceEntry.Property(nameof(IotInterface.Expression));

                if (alias.IsModified) {
                    // TODO: rename references
                }

                if (expression.IsModified || alias.IsModified) {
                    expressions.RefreshInterface(provider, ifaceEntry.Entity);
                }
            }

            return Task.CompletedTask;
        }
    }
}