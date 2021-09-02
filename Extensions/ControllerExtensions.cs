using IotDash.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace IotDash.Extensions {
    public static class ControllerExtensions {
        public static Guid? GetUserId(this HttpContext httpContext) {

            var user = httpContext.User;

            if (user == null) {
                return null;
            }

            return new Guid(user.Claims.Single(c => c.Type == JwtCustomClaimNames.Id).Value);
        }

        public static string GetClaim(this ClaimsPrincipal claims, string type) {
            return claims.Claims.Single(claim => claim.Type == type).Value;
        }
    }
}