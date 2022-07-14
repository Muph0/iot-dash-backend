using Microsoft.AspNetCore.Authorization;

using IotDash.Authorization.Requirements;
using System;

namespace IotDash.Authorization {

    /// <summary>
    /// Contains static generator methods for different policies.
    /// </summary>
    public static class Policies {

        /// <summary>
        /// Adds the <see cref="JwtAuthorized"/> requirement to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void Authorized(AuthorizationPolicyBuilder policy) {
            policy.Requirements.Add(new JwtAuthorized());
            policy.AuthenticationSchemes.Add("Bearer");
        }

        /// <summary>
        /// Adds the <see cref="RouteInterfaceExists"/>
        /// requirement to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void InterfaceAccess(AuthorizationPolicyBuilder policy) {
            policy.Requirements.Add(new RouteInterfaceExists());
        }


        /// <summary>
        /// Adds the <see cref="JwtAuthorized"/>, <see cref="RouteInterfaceExists"/>
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void AuthorizedInterfaceAccess(AuthorizationPolicyBuilder policy) {
            Authorized(policy);
            InterfaceAccess(policy);
        }


        /// <summary>
        /// Adds the <see cref="JwtAuthorized"/>, <see cref="RouteInterfaceExists"/>
        /// <see cref="UserOwnsInterface"/>
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void AuthorizedOwnInterfaceAccess(AuthorizationPolicyBuilder policy) {
            AuthorizedInterfaceAccess(policy);
            policy.Requirements.Add(new UserOwnsInterface());
        }
    }
}