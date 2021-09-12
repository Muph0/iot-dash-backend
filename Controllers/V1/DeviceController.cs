using IotDash.Authorization;
using IotDash.Contracts.V1;
using IotDash.Data;
using IotDash.Data.Model;
using IotDash.Extensions;
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


    public class DeviceController : Controller {

        private IDeviceStore devices;
        private IInterfaceStore interfaces;
        private IIdentityService identity;
        private IUserStore users;

        public string BaseUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";

        public DeviceController(IDeviceStore devices, IIdentityService identity, IInterfaceStore interfaces,
                IUserStore users) {
            this.devices = devices;
            this.interfaces = interfaces;
            this.identity = identity;
            this.users = users;
        }

        [Authorize(Policy = nameof(Policies.Authorized))]
        [HttpGet(ApiRoutes.Device.GetAll)]
        [Produces("application/json", Type = typeof(IEnumerable<Contracts.V1.Model.IotDevice>))]
        public async Task<IActionResult> GetAll() {
            IReadOnlyList<IotDevice> allDevices = await devices.GetAllAsync();
            return Ok(allDevices.Select(d => d.ToContract()));
        }

        [Authorize(Policy = nameof(Policies.AuthorizedDeviceAccess))]
        [HttpGet(ApiRoutes.Device.Get)]
        [Produces("application/json", Type = typeof(Contracts.V1.Model.IotDeviceWInterfaces))]
        public Task<IActionResult> Get() {

            var device = HttpContext.Features.Get<IotDevice>();
            return Task.FromResult<IActionResult>(Ok(device.ToContractDetail()));
        }

        [Authorize(Policy = nameof(Policies.Authorized))]
        [HttpPost(ApiRoutes.Device.Create)]
        [Produces("application/json", Type = typeof(DeviceResponse))]
        public async Task<IActionResult> Create([FromBody] DeviceCreateRequest request) {

            if (!ModelState.IsValid) {
                return DeviceResponse.BadRequest(ModelState.ErrorMessages());
            }

            var owner = await users.GetByEmailAsync(request.OwnerEmail);

            if (owner == null) {
                return DeviceResponse.BadRequest(Error.NoSuchUser());
            }

            IotDevice newDevice = new() {
                Id = Guid.NewGuid(),
                Alias = request.Alias,
                OwnerId = owner.Id,
            };

            await devices.CreateAsync(newDevice);

            int ifaceIdCounter = 0;
            foreach (var ifaceReq in request.Interfaces) {

                IotInterface newIface = new() {
                    Id = ifaceIdCounter++,
                    DeviceId = newDevice.Id,
                    Alias = ifaceReq.Alias,
                    Kind = ifaceReq.Kind,
                };

                await interfaces.CreateAsync(newIface);
            }

            bool written = await devices.SaveChangesAsync();
            Debug.Assert(written);

            var locationUri = BaseUrl + "/" + ApiRoutes.Device.Get.Replace(ApiRoutes.Device.deviceId, newDevice.Id.ToString());
            return Created(locationUri, DeviceResponse.Ok(newDevice.ToContract()));
        }

        [Authorize(Policy = nameof(Policies.AuthorizedOwnDeviceAccess))]
        [HttpPatch(ApiRoutes.Device.Update)]
        [Produces("application/json", Type = typeof(DeviceResponse))]
        public async Task<IActionResult> PatchDevice([FromBody] DevicePatchRequest request) {
            var device = HttpContext.Features.GetRequired<IotDevice>();

            int changedFields = 0;

            if (request.Alias != null) {
                device.Alias = request.Alias;
                changedFields++;
            }

            if (request.OwnerEmail != null) {
                var newOwner = await users.GetByEmailAsync(request.OwnerEmail);
                if (newOwner == null) {
                    return DeviceResponse.BadRequest(Error.NoSuchUser());
                }

                device.OwnerId = newOwner.Id;
                changedFields++;
            }

            if (changedFields == 0) {
                return DeviceResponse.BadRequest(Error.NoModificationsInRequest());
            }

            bool updated = await devices.SaveChangesAsync();
            if (!updated) {
                return DeviceResponse.NotFound(Error.DeviceAlreadyDeleted());
            }

            return DeviceResponse.Ok(device.ToContract());
        }

        [Authorize(Policy = nameof(Policies.AuthorizedOwnDeviceAccess))]
        [HttpDelete(ApiRoutes.Device.Delete)]
        [Produces("application/json", Type = typeof(DeviceResponse))]
        public async Task<IActionResult> Delete() {

            var device = HttpContext.Features.GetRequired<IotDevice>();

            await devices.DeleteByKeyAsync(device.Id);
            bool updated = await devices.SaveChangesAsync();
            if (!updated) {
                return DeviceResponse.NotFound(Error.DeviceAlreadyDeleted());
            }

            return NoContent();
        }

    }

}