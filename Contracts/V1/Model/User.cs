using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Contracts.V1.Model {

    public class User {
        protected readonly IdentityUser user;

        public User(Microsoft.AspNetCore.Identity.IdentityUser user) {
            this.user = user;
        }

        public string Email => user.Email;
        public string Id => user.Id;
        public string UserName => user.UserName;
    }
}