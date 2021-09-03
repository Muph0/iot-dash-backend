using IotDash.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace IotDash.Extensions {
    public static class ContextExtensions {

        public static Guid? GetUserId(this HttpContext httpContext) {

            var user = httpContext.User;
            Debug.Assert(user != null);

            return user.GetUserId();
        }
        public static Guid? GetUserId(this ClaimsPrincipal user) {

            string? id = user.GetClaim(JwtCustomClaimNames.Id);

            if (id != null) {
                return new Guid(id);
            } else {
                return null;
            }
        }

        public static DateTime? GetExpiryDate(this ClaimsPrincipal claims) {
            var n_seconds = claims.GetClaim(JwtRegisteredClaimNames.Exp);
            if (n_seconds == null) {
                return null;
            }
            return DateTime.UnixEpoch.AddSeconds(int.Parse(n_seconds));
        }

        public static string? GetClaim(this ClaimsPrincipal claims, string type) {
            return claims.Claims.SingleOrDefault(claim => claim.Type == type)?.Value;
        }
    }
}