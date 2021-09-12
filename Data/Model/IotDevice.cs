using IotDash.Contracts.V1;
using Microsoft.AspNetCore.Identity;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IotDash.Data.Model {

    public class IotDevice {

        [Key]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string? Alias { get; set; }

        [MaxLength(36)]
        public string OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual IdentityUser Owner { get; set; }

        public virtual ICollection<IotInterface> Interfaces { get; set; }

        public string IpAddress { get; set; }

        public string _JsonTrap => throw new Exception("This object should not be passed to clients.");

        public override string ToString()
            => string.Join(", ", typeof(IotDevice).GetProperties()
                .Where(p => !p.Name.StartsWith("_") && p.GetCustomAttribute<ForeignKeyAttribute>() == null)
                .Select(p => $"{p.Name}={{{p.GetValue(this)}}}"));

    }

}