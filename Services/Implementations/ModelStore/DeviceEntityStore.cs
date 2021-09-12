using IotDash.Data;
using IotDash.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Services.Implementations.ModelStore {

    internal class DeviceEntityStore : IDeviceStore {

        private readonly DataContext db;

        public DeviceEntityStore(DataContext db) {
            this.db = db;
        }

        public async Task<IReadOnlyList<IotDevice>> GetAllAsync() {
            return await db.Devices.ToListAsync();
        }

        public async Task<IotDevice?> GetByKeyAsync(Guid deviceId) {
            return await db.Devices.SingleOrDefaultAsync(d => d.Id == deviceId);
        }

        public Task<IotDevice?> UpdateAsync(IotDevice deviceToUpdate) {
            var entry = db.Devices.Update(deviceToUpdate);
            return Task.FromResult<IotDevice?>(entry.Entity);
        }

        public async Task<bool> DeleteByKeyAsync(Guid deviceId) {
            IotDevice? device = await GetByKeyAsync(deviceId);

            if (device == null) {
                return false;
            }

            db.Devices.Remove(device);
            return true;
        }

        public async Task CreateAsync(IotDevice deviceToCreate) {
            await db.Devices.AddAsync(deviceToCreate);
        }

        public async Task<IotDevice?> UserOwnsDeviceAsync(Guid userId, Guid deviceId) {
            IotDevice device = await db.Devices.SingleOrDefaultAsync(d => d.Id == deviceId);

            if (device == null || device.OwnerId != userId.ToString()) { 
                return null; 
            }

            return device;
        }

        public async Task<bool> SaveChangesAsync() {
            int updatedCount = await db.SaveChangesAsync();
            return updatedCount > 0;
        }
    }

}