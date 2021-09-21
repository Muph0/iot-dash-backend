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
using IotDash.Exceptions;
using IotDash.Extensions.Context;
using System.Linq;

namespace IotDash.Authorization.Requirements {

    class RouteDeviceExists : IAuthorizationRequirement {

        public class Handler : AuthorizationHandler<RouteDeviceExists> {

            private readonly IDeviceStore devices;
            private readonly ILogger logger;

            public Handler(IDeviceStore devices, ILogger<Handler> logger) {
                this.devices = devices;
                this.logger = logger;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, RouteDeviceExists requirement) {

                try {

                    var context = authContext.GetHttpContext();

                    if (authContext.HasFailed) {
                        return;
                    }

                    var user = context.Features.GetRequired<IdentityUser>();
                    var deviceIdStr = context.GetFromRoute<string>(nameof(ApiRoutes.Device.deviceId));
                    Debug.Assert(deviceIdStr != null, this.GetType().Name + " requirement on bad route.");

                    if (!Guid.TryParse(deviceIdStr, out Guid deviceId)) {
                        throw new BadRequestException("Device Id in bad format.");
                    }

                    var device = await devices.GetByKeyAsync(deviceId);

                    if (device != null) {
                        context.Features.Set(device);
                        authContext.Succeed(requirement);
                    } else {
                        throw new ResourceNotFoundException("Given device does not exist.");
                    }

                } finally {
                    logger.LogTrace($"Requirement {(authContext.PendingRequirements.Any(r => r == requirement) ? "failed" : "succeeded")}.");
                }
            }
        }
    }
}