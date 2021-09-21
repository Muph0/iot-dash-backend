using IotDash.Authorization;
using IotDash.Contracts;
using IotDash.Contracts.V1;
using IotDash.Contracts.V1.Model.Extensions;
using IotDash.Data.Model;
using IotDash.Extensions.Context;
using IotDash.Extensions.Error;
using IotDash.Extensions.ObjectMapping;
using IotDash.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Controllers.V1 {


    public class InterfaceController : Controller {

        private readonly IInterfaceStore interfaces;
        private readonly IDeviceStore devices;
        private readonly IIdentityService identity;
        private readonly IUserStore users;
        private readonly IHistoryStore history;

        public string BaseUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";

        public InterfaceController(IDeviceStore devices, IIdentityService identity, IInterfaceStore interfaces,
                 IUserStore users, IHistoryStore history) {
            this.interfaces = interfaces;
            this.devices = devices;
            this.identity = identity;
            this.users = users;
            this.history = history;
        }

        [Authorize(Policy = nameof(Policies.AuthorizedInterfaceAccess))]
        [HttpGet(ApiRoutes.Device.Interface.Get)]
        [Produces(MimeType.Application_JSON, Type = typeof(Contracts.V1.Model.IotInterface))]
        public Task<IActionResult> GetInterface() {

            var iface = HttpContext.Features.Get<IotInterface>();
            return Task.FromResult<IActionResult>(
                Ok(iface.ToContract())
            );
        }

        [Authorize(Policy = nameof(Policies.AuthorizedDeviceAccess))]
        [HttpPost(ApiRoutes.Device.Interface.Create)]
        [Produces(MimeType.Application_JSON, Type = typeof(IEnumerable<Contracts.V1.InterfaceResponse>))]
        public async Task<IActionResult> CreateInterface([FromBody] InterfaceCreateRequest request) {

            var device = HttpContext.Features.GetRequired<IotDevice>();
            int ifaceId = device.Interfaces.Count;

            IotInterface newIface = request.CreateModel(device.Id, ifaceId);
            await interfaces.CreateAsync(newIface);
            await interfaces.SaveChangesAsync();

            return InterfaceResponse.Ok(newIface.ToContract());
        }


        [Authorize(Policy = nameof(Policies.AuthorizedOwnInterfaceAccess))]
        [HttpPatch(ApiRoutes.Device.Interface.Update)]
        [Produces(MimeType.Application_JSON, Type = typeof(InterfaceResponse))]
        public async Task<IActionResult> UpdateInterface([FromBody] InterfacePatchRequest request) {

            var iface = HttpContext.Features.GetRequired<IotInterface>();

            if (!ModelState.IsValid) {
                return InterfaceResponse.BadRequest(ModelState.ErrorMessages());
            }

            if (request.Expression != null && iface.Kind.IsReadOnly()) {
                return InterfaceResponse.BadRequest(Error.ExpressionOnReadOnlyIface());
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


        [Authorize(Policy = nameof(Policies.AuthorizedOwnInterfaceAccess))]
        [HttpDelete(ApiRoutes.Device.Interface.Delete)]
        [Produces(MimeType.Application_JSON, Type = typeof(InterfaceResponse))]
        public async Task<IActionResult> DeleteInterface([FromBody] InterfacePatchRequest request) {

            var iface = HttpContext.Features.GetRequired<IotInterface>();

            bool ok = await interfaces.DeleteByKeyAsync((iface.DeviceId, iface.Id));
            if (!(ok && await interfaces.SaveChangesAsync())) {
                InterfaceResponse.NotFound(Error.DeviceAlreadyDeleted());
            }
            return InterfaceResponse.NoContent();
        }


        [Authorize(Policy = nameof(Policies.AuthorizedOwnInterfaceAccess))]
        [HttpPost(ApiRoutes.Device.Interface.History)]
        [Produces(MimeType.Application_JSON, Type = typeof(HistoryResponse))]
        public async Task<IActionResult> GetHistory([FromBody] HistoryRequest request) {

            if (!ModelState.IsValid) {
                return HistoryResponse.BadRequest(ModelState.ErrorMessages());
            }

            var iface = HttpContext.Features.GetRequired<IotInterface>();
            var results = await history.GetPagedHistoryAsync(iface, request);

            return HistoryResponse.Ok(results.Select(r => r.ToContract()));
        }


        [Authorize(Policy = nameof(Policies.AuthorizedOwnInterfaceAccess))]
        [HttpPut(ApiRoutes.Device.Interface.History)]
        public async Task<IActionResult> GenerateHistory() {

            var iface = HttpContext.Features.GetRequired<IotInterface>();

            DateTime start = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            DateTime end = DateTime.Now;

            var delta = end - start;
            for (int m = 0; m < delta.TotalMinutes; m++) {

                double value = Math.Sin(m / 1000.0) * 10 + 3;

                HistoryEntry e = new() {
                    Average = value,
                    Min = value,
                    Max = value,
                    DeviceId = iface.DeviceId,
                    InterfaceId = iface.Id,
                    When = start.AddMinutes(m),
                };

                await history.CreateAsync(e);
            }

            await history.SaveChangesAsync();

            return Ok();
        }
    }
}