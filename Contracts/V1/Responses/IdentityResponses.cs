using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace IotDash.Contracts.V1 {

    /// <summary>
    /// Represents result of an authentication
    /// </summary>
    public class AuthResponse : StatusResponse<(string Token, string RefreshToken), AuthResponse> {

        /// <summary>
        /// Newly issued JWT token.
        /// </summary>
        public string? Token => Value.Token;

        /// <summary>
        /// One-time use refresh token.
        /// </summary>
        public string? RefreshToken => Value.RefreshToken;
    }
}