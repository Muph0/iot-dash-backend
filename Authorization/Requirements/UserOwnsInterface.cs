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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using IotDash.Contracts.V1;
using IotDash.Data.Model;
using IotDash.Utils.Context;
using System.Linq;
using Microsoft.AspNetCore.Http.Features;

namespace IotDash.Authorization.Requirements {

    /// <summary>
    /// <see cref="IAuthorizationRequirement"/> that succeeds if user in <see cref="HttpContext.Features"/> owns the device in the same <see cref="IFeatureCollection"/>.
    /// </summary>
    class UserOwnsInterface : IAuthorizationRequirement {

        /// <summary>
        /// Handler for <see cref="UserOwnsInterface"/>.
        /// </summary>
        public class Handler : AuthorizationHandler<Requirements.UserOwnsInterface> {

            private readonly ILogger<Handler> logger;

            public Handler(ILogger<Handler> logger) {
                this.logger = logger;
            }

            protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext, UserOwnsInterface requirement) {

                try {

                    var context = authContext.GetHttpContext();
                    
                    var user = context.Features.GetRequired<IdentityUser>();
                    var iface = context.Features.GetRequired<IotInterface>();

                    if (iface.OwnerId == user.Id) {
                        authContext.Succeed(requirement);
                    }

                } finally {
                    logger.LogTrace($"Requirement {(authContext.PendingRequirements.Any(r => r == requirement) ? "failed" : "succeeded")}.");
                }

                return Task.CompletedTask;
            }
        }
    }
}