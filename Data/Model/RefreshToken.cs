using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IotDash.Data.Model {
    public class RefreshToken : ModelObject {

        /// <summary>
        /// The actual data of the token.
        /// </summary>
        [Key]
        public string Token { get; set; }

        /// <summary>
        /// The <c>jti</c> of the associate JWT.
        /// </summary>
        public string JwtId { get; set; }

        /// <summary>
        /// When was this token created.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// When will this token expire.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// True if the token has been used.
        /// </summary>
        public bool Used { get; set; }
        /// <summary>
        /// True if the token is not valid.
        /// </summary>
        public bool Invalidated { get; set; }

        /// <summary>
        /// The <c>id</c> of the user who generated this token.
        /// </summary>
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }
    }
}