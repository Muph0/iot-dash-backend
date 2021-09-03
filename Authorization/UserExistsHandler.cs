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
using IotDash.Services;
using IotDash.Extensions;

namespace IotDash.Authorization {

    namespace Requirements {
        class UserExists : IAuthorizationRequirement { }
    }

    class UserExistsHandler : AuthorizationHandler<Requirements.UserExists> {

        private readonly IIdentityService identityService;

        public UserExistsHandler(IIdentityService identityService) {
            this.identityService = identityService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, Requirements.UserExists requirement) {

            Debug.Assert(authContext.Resource != null);
            HttpContext context = (HttpContext)authContext.Resource;

            var n_userId = context.GetUserId();
            if (n_userId == null) {
                return;
            }

            Guid userId = (Guid)n_userId;
            var user = await identityService.GetUserByIdAsync(userId);

            if (user != null) {
                authContext.Succeed(requirement);
            }
        }
    }

}