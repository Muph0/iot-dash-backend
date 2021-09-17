using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IotDash.Contracts.V1.Model {
    public class IotDeviceWInterfaces : IotDevice {
        public IotDeviceWInterfaces(Data.Model.IotDevice device) : base(device) { }
        [Required]
        public IEnumerable<IotInterface> Interfaces => device.Interfaces.Select(i => i.ToContract());
    }
}