using IotDash.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace IotDash.Utils.Context {
    public static class ContextExtensions {

        public static string? GetUserId(this HttpContext httpContext) {
            var user = httpContext.User;
            Debug.Assert(user != null);
            return user.GetUserId();
        }
        public static string? GetUserId(this ClaimsPrincipal user) {
            string? id = user.GetClaim(JwtCustomClaimNames.Id);
            return id;
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

        public static HttpContext GetHttpContext(this AuthorizationHandlerContext context) {
            var result = context.Resource as HttpContext;
            Debug.Assert(result != null);
            return result;
        }

        public static T? GetFromRoute<T>(this HttpContext context, string key) {

            if (context.GetRouteData().Values.TryGetValue(key, out var value)) {
                return (T?)value;
            }
            return default(T?);
        }

        public static T GetRequired<T>(this IFeatureCollection features) {
            T result = features.Get<T>();
            if (result == null) {
                throw new ArgumentException($"Feature '{typeof(T).FullName}' is missing from the feature collection.");
            }
            return result;
        }
    }
}