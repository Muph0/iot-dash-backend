using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1.Model {

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InterfaceKind {
        Probe, 
        Switch
    }

    public class IotInterface {
        protected readonly Data.Model.IotInterface iface;

        public IotInterface(Data.Model.IotInterface iface) {
            this.iface = iface;
        }

        [Required]
        public int Id => iface.Id;
        [Required]
        public Guid DeviceId => iface.DeviceId;
        public string? Alias => iface.Alias;
        [Required]
        public InterfaceKind Kind => iface.Kind;
        public string? Expression => iface.Expression;
        [Required]
        public double Value => iface.Value;
    }
}