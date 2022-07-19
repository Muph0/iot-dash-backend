using Microsoft.AspNetCore.Authorization;

using IotDash.Authorization.Requirements;
using System;

namespace IotDash.Authorization {

    /// <summary>
    /// Contains static generator methods for different policies.
    /// <para>
    /// Pass a <see cref="AuthorizationPolicyBuilder"/> to any of the member methods to apply the relevant policy.
    /// </para>
    /// </summary>
    public static class Policies {

        /// <summary>
        /// Adds the <see cref="Requirements.JwtAuthorized"/> requirement to given <paramref name="policy"/> builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void Authorized(AuthorizationPolicyBuilder policy) {
            policy.Requirements.Add(new JwtAuthorized());
            policy.AuthenticationSchemes.Add("Bearer");
        }

        /// <summary>
        /// Adds the <see cref="Requirements.RouteInterfaceExists"/>
        /// requirement to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void InterfaceAccess(AuthorizationPolicyBuilder policy) {
            policy.Requirements.Add(new RouteInterfaceExists());
        }


        /// <summary>
        /// Adds the <see cref="Requirements.JwtAuthorized"/>, <see cref="Requirements.RouteInterfaceExists"/>
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void AuthorizedInterfaceAccess(AuthorizationPolicyBuilder policy) {
            Authorized(policy);
            InterfaceAccess(policy);
        }


        /// <summary>
        /// Adds the <see cref="Requirements.JwtAuthorized"/>, 
        /// <see cref="Requirements.RouteInterfaceExists"/> and
        /// <see cref="Requirements.UserOwnsInterface"/>
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void AuthorizedOwnInterfaceAccess(AuthorizationPolicyBuilder policy) {
            AuthorizedInterfaceAccess(policy);
            policy.Requirements.Add(new UserOwnsInterface());
        }
    }
}