using System;
using System.Linq;

namespace IotDash.Contracts.V1.Model {
    public class IotDevice {
        protected readonly Data.Model.IotDevice device;

        public IotDevice(Data.Model.IotDevice device) {
            this.device = device;
        }

        public Guid Id => device.Id;
        public string? Alias => device.Alias;
        public string IpAddress => device.IpAddress;
        public string OwnerId => device.OwnerId;
        public int InterfaceCount => device.Interfaces.Count;
    }
}