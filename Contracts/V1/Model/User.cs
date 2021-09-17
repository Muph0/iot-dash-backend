using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Contracts.V1.Model {

    public class User {
        protected readonly IdentityUser user;

        public User(Microsoft.AspNetCore.Identity.IdentityUser user) {
            this.user = user;
        }

        [Required]
        public string Email => user.Email;
        [Required]
        public string Id => user.Id;
        [Required]
        public string UserName => user.UserName;
    }
}