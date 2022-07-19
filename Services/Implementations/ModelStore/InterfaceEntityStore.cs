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

namespace IotDash.Services.ModelStore {

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class InterfaceEntityStore : IInterfaceStore {

        private readonly DataContext db;
        private readonly ILogger logger;

        public InterfaceEntityStore(DataContext db, ILogger<InterfaceEntityStore> logger) {
            this.db = db;
            this.logger = logger;
        }

        public async Task CreateAsync(IotInterface ifaceToCreate) {
            await db.Interfaces.AddAsync(ifaceToCreate);
        }

        public async Task<bool> DeleteByKeyAsync(Guid key) {
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

        public async Task<IotInterface?> GetByKeyAsync(Guid key) {
            return await db.Interfaces.SingleOrDefaultAsync(i => i.Id == key);
        }

        public async Task<bool> SaveChangesAsync() {
            int affected = await db.SaveChangesAsync();
            return affected > 0;
        }

    }
}