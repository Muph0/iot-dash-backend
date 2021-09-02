using System.ComponentModel.DataAnnotations;

namespace IotDash.Contracts.V1 {
    public class UserRegistrationRequest {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginRequest {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenRequest {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
