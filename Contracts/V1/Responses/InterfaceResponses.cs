using IotDash.Contracts.V1.Model;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1 {


    public class InterfaceResponse : StatusResponse<IotInterface, InterfaceResponse> {
        public IotInterface? Interface => Value;
    }
}