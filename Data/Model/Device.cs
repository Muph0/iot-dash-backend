using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IotDash.Data.Model {

    public class Device {

        [Key]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string? Alias { get; set; }

        [MaxLength(1 << 16)]
        [Column(TypeName = "TEXT")]
        public string? Expression { get; set; }

        [MaxLength(36)]
        public string OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public IdentityUser Owner { get; set; }

    }

}