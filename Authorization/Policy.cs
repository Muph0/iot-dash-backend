using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace IotDash.Authorization {

    public static class Policies {
    
        public static void Default(AuthorizationPolicyBuilder policy) {

            policy.Requirements.Add(new Requirements.UserExists());

        }

    }

}