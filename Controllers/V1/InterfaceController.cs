using IotDash.Authorization;
using IotDash.Contracts;
using IotDash.Contracts.V1;
using IotDash.Data;
using IotDash.Data.Model;
using IotDash.Extensions;
using IotDash.Extensions.ObjectMapping;
using IotDash.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Controllers.V1 {


    public class InterfaceController : Controller {

        private readonly IInterfaceStore interfaces;
        private readonly IDeviceStore devices;
        private readonly IIdentityService identity;
        private readonly IUserStore users;

        public string BaseUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";

        public InterfaceController(IDeviceStore devices, IIdentityService identity, IInterfaceStore interfaces,
                 IUserStore users) {
            this.interfaces = interfaces;
            this.devices = devices;
            this.identity = identity;
            this.users = users;
        }

        [Authorize(Policy = nameof(Policies.AuthorizedInterfaceAccess))]
        [HttpGet(ApiRoutes.Device.Interface.Get)]
        [Produces(MimeType.Application_JSON, Type = typeof(IEnumerable<Contracts.V1.Model.IotDevice>))]
        public Task<IActionResult> GetInterface() {

            var iface = HttpContext.Features.Get<IotInterface>();
            return Task.FromResult<IActionResult>(
                Ok(iface.ToContract())
            );
        }


        [Authorize(Policy = nameof(Policies.AuthorizedOwnInterfaceAccess))]
        [HttpPatch(ApiRoutes.Device.Interface.Update)]
        [Produces(MimeType.Application_JSON, Type = typeof(InterfaceResponse))]
        public async Task<IActionResult> UpdateInterface([FromBody] InterfacePatchRequest request) {

            var iface = HttpContext.Features.GetRequired<IotInterface>();

            if (!ModelState.IsValid) {
                return InterfaceResponse.BadRequest(ModelState.ErrorMessages());
            }

            int updatedFields = request.CopyTo(iface);

            if (updatedFields == 0) {
                InterfaceResponse.BadRequest(Error.NoModificationsInRequest());
            }

            bool ok = await interfaces.SaveChangesAsync();

            if (!ok) {
                InterfaceResponse.NotFound(Error.DeviceAlreadyDeleted());
            }

            return InterfaceResponse.Ok(iface.ToContract());
        }

    }

}