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
using IotDash.Utils;
using IotDash.Exceptions;
using System.Linq;
using IotDash.Utils.Context;

namespace IotDash.Authorization.Requirements {


    /// <summary>
    /// Authorization requirement that checks succeeds if JWT subject is a valid user.
    /// </summary>
    public class JwtAuthorized : IAuthorizationRequirement {

        /// <summary>
        /// Handler for <see cref="Requirements.JwtAuthorized"/> requirement.
        /// </summary>
        public class Handler : AuthorizationHandler<JwtAuthorized> {

            private readonly IUserStore users;
            private readonly ILogger logger;

            public Handler(IUserStore users, ILogger<Handler> logger) {
                this.users = users;
                this.logger = logger;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, JwtAuthorized requirement) {

                try {
                    var context = authContext.GetHttpContext();
                    var id = context.GetUserId();

                    if (id != null) {
                        var user = await users.GetByKeyAsync(id);

                        if (user != null) {
                            context.Features.Set(user);
                            authContext.Succeed(requirement);
                            return;
                        }
                    }

                    authContext.Fail();

                } finally {
                    logger.LogTrace($"Requirement {(authContext.PendingRequirements.Any(r => r == requirement) ? "failed" : "succeeded")}.");
                }
            }
        }
    }
}