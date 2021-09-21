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
        /// Adds the <see cref="RouteDeviceExists"/> requirement to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void DeviceAccess(AuthorizationPolicyBuilder policy) {
            policy.Requirements.Add(new RouteDeviceExists());
        }

        /// <summary>
        /// Adds the <see cref="JwtAuthorized"/>, <see cref="RouteDeviceExists"/> 
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void AuthorizedDeviceAccess(AuthorizationPolicyBuilder policy) {
            Authorized(policy);
            DeviceAccess(policy);
        }

        /// <summary>
        /// Adds the <see cref="JwtAuthorized"/>, <see cref="RouteDeviceExists"/>, <see cref="UserOwnsDevice"/> 
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void AuthorizedOwnDeviceAccess(AuthorizationPolicyBuilder policy) {
            AuthorizedDeviceAccess(policy);
            policy.Requirements.Add(new UserOwnsDevice());
        }

        /// <summary>
        /// Adds the <see cref="RouteDeviceExists"/>, <see cref="RouteInterfaceExists"/>
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void InterfaceAccess(AuthorizationPolicyBuilder policy) {
            DeviceAccess(policy);
            policy.Requirements.Add(new RouteInterfaceExists());
        }


        /// <summary>
        /// Adds the <see cref="JwtAuthorized"/>, <see cref="RouteDeviceExists"/>, <see cref="RouteInterfaceExists"/>
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void AuthorizedInterfaceAccess(AuthorizationPolicyBuilder policy) {
            Authorized(policy);
            InterfaceAccess(policy);
        }


        /// <summary>
        /// Adds the <see cref="JwtAuthorized"/>, <see cref="RouteDeviceExists"/>, <see cref="RouteInterfaceExists"/>
        /// <see cref="UserOwnsDevice"/>
        /// requirements to given policy builder.
        /// </summary>
        /// <param name="policy">Builder to add to.</param>
        public static void AuthorizedOwnInterfaceAccess(AuthorizationPolicyBuilder policy) {
            AuthorizedInterfaceAccess(policy);
            policy.Requirements.Add(new UserOwnsDevice());
        }
    }
}