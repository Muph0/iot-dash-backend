using IotDash.Data.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IotDash.Services {

    public interface IDeviceStore {

        Task<IReadOnlyList<Device>> GetAllAsync();
        Task<Device> GetByIdAsync(Guid deviceId);
        Task<bool> CreateAsync(Device deviceToCreate);
        Task<bool> UpdateAsync(Device deviceToUpdate);
        Task<bool> DeleteAsync(Guid deviceId);
        Task<bool> UserOwnsDevice(string v, Guid deviceId);
    }

}