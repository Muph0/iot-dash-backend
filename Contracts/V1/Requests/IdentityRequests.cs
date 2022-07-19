using System.ComponentModel.DataAnnotations;

namespace IotDash.Contracts.V1 {
    public class UserRegistrationRequest {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class UserLoginRequest {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RefreshTokenRequest {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
