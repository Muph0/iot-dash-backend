using System.Collections.Generic;
using System.Linq;

namespace IotDash.Contracts.V1.Model {
    public class IotDeviceWInterfaces : IotDevice {
        public IotDeviceWInterfaces(Data.Model.IotDevice device) : base(device) { }
        public IEnumerable<IotInterface> Interfaces => device.Interfaces.Select(i => i.ToContract());
    }
}