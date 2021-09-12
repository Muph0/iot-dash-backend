using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using IotDash.Services;
using IotDash.Extensions;
using Microsoft.AspNetCore.Identity;
using IotDash.Data.Model;
using IotDash.Contracts.V1;
using IotDash.Exceptions;

namespace IotDash.Authorization.Requirements {

    class RouteInterfaceExists : IAuthorizationRequirement {

        public class Handler : AuthorizationHandler<RouteInterfaceExists> {

            private readonly IInterfaceStore interfaces;

            public Handler(IInterfaceStore interfaces) {
                this.interfaces = interfaces;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, RouteInterfaceExists requirement) {

                var context = authContext.GetHttpContext();

                if (authContext.HasFailed) {
                    return;
                }

                IdentityUser user = context.Features.GetRequired<IdentityUser>();
                IotDevice device = context.Features.GetRequired<IotDevice>();

                var ifaceIdStr = context.GetFromRoute<string>(nameof(ApiRoutes.Device.Interface.ifaceId));
                Debug.Assert(ifaceIdStr != null, this.GetType().Name + " requirement on bad route.");

                if (!int.TryParse(ifaceIdStr, out int ifaceId)) {
                    throw new BadRequestException("Interface id must be a number.");
                }

                var iface = await interfaces.GetByKeyAsync((device.Id, ifaceId));

                if (iface != null) {
                    context.Features.Set(iface);
                    authContext.Succeed(requirement);
                } else {
                    throw new ResourceNotFoundException("Given interface does not exist.");
                }
            }
        }
    }
}