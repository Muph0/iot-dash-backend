using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public double Min { get; set; }
        public double Max { get; set; }
        public double Average { get; set; }

        [ForeignKey(nameof(InterfaceId))]
        public virtual IotInterface Interface { get; set; }
    }
}