using Microsoft.AspNetCore.Authorization;

using IotDash.Authorization.Requirements;
using System;

namespace IotDash.Authorization {

    public static class Policies {

        public static void Authorized(AuthorizationPolicyBuilder policy) {
            policy.Requirements.Add(new JwtAuthorized());
            policy.AuthenticationSchemes.Add("Bearer");
        }

        public static void DeviceAccess(AuthorizationPolicyBuilder policy) {
            policy.Requirements.Add(new RouteDeviceExists());
        }

        public static void AuthorizedDeviceAccess(AuthorizationPolicyBuilder policy) {
            Authorized(policy);
            DeviceAccess(policy);
        }

        public static void AuthorizedOwnDeviceAccess(AuthorizationPolicyBuilder policy) {
            AuthorizedDeviceAccess(policy);
            policy.Requirements.Add(new UserOwnsDevice());
        }

        public static void InterfaceAccess(AuthorizationPolicyBuilder policy) {
            DeviceAccess(policy);
            policy.Requirements.Add(new RouteInterfaceExists());
        }

        public static void AuthorizedInterfaceAccess(AuthorizationPolicyBuilder policy) {
            Authorized(policy);
            InterfaceAccess(policy);
        }

        public static void AuthorizedOwnInterfaceAccess(AuthorizationPolicyBuilder policy) {
            AuthorizedInterfaceAccess(policy);
            policy.Requirements.Add(new UserOwnsDevice());
        }
    }
}