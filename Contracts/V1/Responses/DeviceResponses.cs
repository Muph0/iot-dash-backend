using IotDash.Contracts.V1.Model;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1 {


    public class DeviceResponse : StatusResponse<IotDevice, DeviceResponse> {

        public IotDevice? Device => Value;


    }
}