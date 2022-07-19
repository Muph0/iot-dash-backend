using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using IotDash.Services;
using IotDash.Utils;
using Microsoft.AspNetCore.Identity;
using IotDash.Data.Model;
using IotDash.Contracts.V1;
using IotDash.Exceptions;
using IotDash.Utils.Context;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;

namespace IotDash.Authorization.Requirements {

    /// <summary>
    /// <see cref="IAuthorizationRequirement"/> that succeeds if the Http route contains a valid interface.
    /// </summary>
    class RouteInterfaceExists : IAuthorizationRequirement {

        /// <summary>
        /// Handler for <see cref="RouteInterfaceExists"/>.
        /// </summary>
        public class Handler : AuthorizationHandler<RouteInterfaceExists> {

            private readonly IInterfaceStore interfaces;
            private readonly ILogger logger;

            public Handler(IInterfaceStore interfaces, ILogger<Handler> logger) {
                this.interfaces = interfaces;
                this.logger = logger;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, RouteInterfaceExists requirement) {

                try {
                    var context = authContext.GetHttpContext();

                    if (authContext.HasFailed) {
                        return;
                    }

                    IdentityUser user = context.Features.GetRequired<IdentityUser>();

                    var ifaceIdStr = context.GetFromRoute<string>(nameof(ApiRoutes.Interface.ifaceId));
                    Debug.Assert(ifaceIdStr != null, this.GetType().Name + " requirement on bad route.");

                    if (!Guid.TryParse(ifaceIdStr, out var ifaceId)) {
                        throw new BadRequestException("Interface id must be a GUID.");
                    }

                    var iface = await interfaces.GetByKeyAsync(ifaceId);

                    if (iface != null) {
                        context.Features.Set(iface);
                        authContext.Succeed(requirement);
                    } else {
                        throw new ResourceNotFoundException("Given interface does not exist.");
                    }

                } finally {
                    logger.LogTrace($"Requirement {(authContext.PendingRequirements.Any(r => r == requirement) ? "failed" : "succeeded")}.");
                }
            }
        }
    }
}