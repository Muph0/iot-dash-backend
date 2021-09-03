using IotDash.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Services {

    public class InMemoryDeviceStore : IDeviceStore {

        private readonly List<Device> _devices;

        public InMemoryDeviceStore() {
            _devices = new();
            for (var i = 0; i < 5; i++) {
                _devices.Add(new Device {
                    Id = Guid.NewGuid(),
                    Alias = $"Device name {i}",
                });
            }
        }

        public Task<bool> CreateAsync(Device deviceToCreate) {
            _devices.Add(deviceToCreate);
            return Task.FromResult(true);
        }

        public Task<Device?> GetByIdAsync(Guid deviceId) {
            return Task.FromResult(_devices.SingleOrDefault(d => d.Id == deviceId));
        }

        public Task<IReadOnlyList<Device>> GetAllAsync() {
            return Task.FromResult<IReadOnlyList<Device>>(_devices.AsReadOnly());
        }

        public Task<bool> UpdateAsync(Device deviceToUpdate) {
            var index = _devices.FindIndex(d => d.Id == deviceToUpdate.Id);
            bool exists = index >= 0;

            if (!exists) {
                return Task.FromResult(false);
            }

            _devices[index] = deviceToUpdate;
            return Task.FromResult(true);
        }

        public async Task<bool> DeleteAsync(Guid deviceId) {

            Device? device = await GetByIdAsync(deviceId);
            if (device == null) {
                return false;
            }

            return _devices.Remove(device);
        }

        public async Task<Device?> UserOwnsDeviceAsync(Guid userId, Guid deviceId) {
            var device = await GetByIdAsync(deviceId);

            if (device == null || device.OwnerId != userId.ToString()) {
                return null;
            }

            return device;
        }
    }

}