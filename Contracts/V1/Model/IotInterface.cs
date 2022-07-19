using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1.Model {

    /// <summary>
    /// Enumeration of different device types as presented over Rest API.
    /// </summary>

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InterfaceKind {
        Probe, 
        Switch
    }

    namespace Extensions {
        public static class InterfaceKindExtensions {
            public static bool IsReadOnly(this InterfaceKind kind) {
                switch (kind) {
                    default: return true;

                    case InterfaceKind.Switch:
                        return false;
                }
            }
        }
    }


    /// <summary>
    /// Represents an Iot device.
    /// </summary>
    public class IotInterface {
        protected readonly Data.Model.IotInterface iface;

        public IotInterface(Data.Model.IotInterface iface) {
            this.iface = iface;
        }

        [Required]
        public Guid Id => iface.Id;
        
        public string? Topic => iface.Topic;
        [Required]
        public InterfaceKind Kind => iface.Kind;
        public string? Expression => iface.Expression;
        [Required]
        public double Value => iface.Value;
        [Required]
        public bool HistoryEnabled => iface.HistoryEnabled;
    }
}