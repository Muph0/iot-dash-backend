using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IotDash.Contracts.V1.Model {

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

    public class IotInterface {
        protected readonly Data.Model.IotInterface iface;

        public IotInterface(Data.Model.IotInterface iface) {
            this.iface = iface;
        }

        public int Id => iface.Id;
        public Guid DeviceId => iface.DeviceId;
        public string? Alias => iface.Alias;
        public InterfaceKind Kind => iface.Kind;
        public string? Expression => iface.Expression;
        public double Value => iface.Value;
        public bool LogHistory => iface.LogHistory;
    }
}