using System.Collections.Generic;

namespace IotDash.Contracts.V1 {
    public class AuthSuccessResponse {
        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }

    public class AuthFailResponse {
        public IEnumerable<string> Errors { get; set; }
    }
}