using IotDash.Contracts.V1;
using IotDash.Contracts.V1.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace IotDash.Data.Model {

    public class IotInterface {

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


        public string _JsonTrap => throw new Exception("This object should not be passed to clients.");

        public override string ToString()
            => string.Join(", ", typeof(IotInterface).GetProperties()
                .Where(p => !p.Name.StartsWith("_") && p.GetCustomAttribute<ForeignKeyAttribute>() == null)
                .Select(p => $"{p.Name}={{{p.GetValue(this)}}}"));


    }

}