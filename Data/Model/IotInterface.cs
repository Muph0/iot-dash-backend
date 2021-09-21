using IotDash.Contracts.V1;
using IotDash.Contracts.V1.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace IotDash.Data.Model {

    public class IotInterface : ModelObject {

        public int Id { get; set; }
        public Guid DeviceId { get; set; }

        [MaxLength(255)]
        public string? Alias { get; set; }

        public InterfaceKind Kind { get; set; }

        [MaxLength(1 << 16)]
        [Column(TypeName = "TEXT")]
        public string? Expression { get; set; }

        public double Value { get; set; }

        [ForeignKey(nameof(DeviceId))]
        public virtual IotDevice Device { get; set; }

        public bool LogHistory { get; set; } = false;

        internal string GetStandardTopic() {
            return $"dev/{DeviceId}/{Id}";
        }

        internal string? GetAliasTopic() {
            if (Alias == null | Device.Alias == null) {
                return null;
            }

            return $"{Device.Alias}/{Alias}";
        }

        internal (Guid, int) GetKey() {
            return (DeviceId, Id);
        }
    }

}