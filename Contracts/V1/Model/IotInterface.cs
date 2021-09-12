using System;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1.Model {
    public class IotInterface {
        protected readonly Data.Model.IotInterface iface;

        public IotInterface(Data.Model.IotInterface iface) {
            this.iface = iface;
        }

        public int Id => iface.Id;
        public Guid DeviceId => iface.DeviceId;
        public string? Alias => iface.Alias;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InterfaceKind Kind => iface.Kind;
        public string? Expression => iface.Expression;
        public double Value => iface.Value;
    }
}