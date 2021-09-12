using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IotDash.Data.Model {
    public class RefreshToken {

        [Key]
        public string Token { get; set; }
        public string JwtId { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public bool Used { get; set; }
        public bool Invalidated { get; set; }

        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }


        public string _JsonTrap => throw new Exception("This object should not be passed to clients.");
    }
}