using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace IotDash.Contracts.V1 {
    //public class AuthSuccessResponse {
    //    public string Token { get; set; }
    //
    //    public string RefreshToken { get; set; }
    //}
    //
    //public class AuthFailResponse {
    //    public IEnumerable<string> Errors { get; set; }
    //}

    public class AuthResponse : StatusResponse<(string Token, string RefreshToken), AuthResponse> {

        public string Token => Value.Token;
        public string RefreshToken => Value.RefreshToken;
    }
}