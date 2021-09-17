using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IotDash.Contracts.V1.Model {

    public class IotDevice {
        protected readonly Data.Model.IotDevice device;

        public IotDevice(Data.Model.IotDevice device) {
            this.device = device;
        }

        [Required]
        public Guid Id => device.Id;
        public string? Alias => device.Alias;
        [Required]
        public string IpAddress => device.IpAddress;
        [Required]
        public string OwnerId => device.OwnerId;
        [Required]
        public int InterfaceCount => device.Interfaces.Count;
    }
}