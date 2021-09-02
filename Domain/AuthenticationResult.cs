using System.Collections.Generic;
using System.Linq;

namespace IotDash.Domain {

    public class AuthenticationResult : IOperationResult {

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public static AuthenticationResult Success(string token, string refreshToken) {
            var result = new AuthenticationResult {
                Errors = new string[0],
                IsSuccess = true,
                Token = token,
                RefreshToken = refreshToken,
            };
            return result;
        }

        public static AuthenticationResult Fail(string errorMessage) => Fail(new[] { errorMessage });
        public static AuthenticationResult Fail(IEnumerable<string> errors) {
            var result = new AuthenticationResult {
                Errors = errors.Select(x => x),
                IsSuccess = false,
                Token = null,
                RefreshToken = null,
            };
            return result;
        }
    }
}