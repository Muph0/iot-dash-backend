using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace IotDash.Data.Model {
    public class HistoryEntry : ModelObject {

        [Key]
        public int Id { get; set; }
        public Guid InterfaceId { get; set; }

        [NotMapped]
        private DateTime when;

        public DateTime WhenUTC {
            get => when.ToUniversalTime();
            set => when = value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
        }

        [NotNull]
        public double? Min { get; set; } = null;
        [NotNull]
        public double? Max { get; set; } = null;
        [NotNull]
        public double? Average { get; set; } = null;

        [ForeignKey(nameof(InterfaceId))]
        public virtual IotInterface Interface { get; set; }
    }
}