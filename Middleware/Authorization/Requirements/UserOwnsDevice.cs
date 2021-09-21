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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using IotDash.Contracts.V1;
using IotDash.Data.Model;
using IotDash.Extensions.Context;
using System.Linq;

namespace IotDash.Authorization.Requirements {

    class UserOwnsDevice : IAuthorizationRequirement {

        public class Handler : AuthorizationHandler<Requirements.UserOwnsDevice> {

            private readonly ILogger<Handler> logger;

            public Handler(ILogger<Handler> logger) {
                this.logger = logger;
            }

            protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext, UserOwnsDevice requirement) {

                try {

                    var context = authContext.GetHttpContext();

                    var user = context.Features.GetRequired<IdentityUser>();
                    var device = context.Features.GetRequired<IotDevice>();

                    if (device.OwnerId == user.Id) {
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