using IotDash.Data;
using IotDash.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Services {

    public class EntityDeviceStore : IDeviceStore {

        private readonly DataContext _dataContext;

        public EntityDeviceStore(DataContext dataContext) {
            _dataContext = dataContext;
        }

        public async Task<IReadOnlyList<Device>> GetAllAsync() {
            return await _dataContext.Devices.ToListAsync();
        }

        public async Task<Device> GetByIdAsync(Guid deviceId) {
            return await _dataContext.Devices.SingleOrDefaultAsync(d => d.Id == deviceId);
        }

        public async Task<bool> UpdateAsync(Device deviceToUpdate) {
            _dataContext.Devices.Update(deviceToUpdate);
            int updatedCount = await _dataContext.SaveChangesAsync();

            return updatedCount > 0;
        }

        public async Task<bool> DeleteAsync(Guid deviceId) {
            Device device = await GetByIdAsync(deviceId);

            if (device == null) {
                return false;
            }

            _dataContext.Remove(device);
            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateAsync(Device deviceToCreate) {
            await _dataContext.Devices.AddAsync(deviceToCreate);
            int createdCount = await _dataContext.SaveChangesAsync();

            return createdCount > 0;
        }

        public async Task<bool> UserOwnsDevice(string userId, Guid deviceId) {
            Device device = await _dataContext.Devices.AsNoTracking().SingleOrDefaultAsync(d => d.Id == deviceId);

            if (device == null) { 
                return false; }

            return device.OwnerId == userId;
        }
    }

}