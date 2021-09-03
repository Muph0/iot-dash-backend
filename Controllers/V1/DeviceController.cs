using IotDash.Contracts.V1;
using IotDash.Data.Model;
using IotDash.Extensions;
using IotDash.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IotDash.Controllers.V1 {

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DeviceController : Controller {

        private IDeviceStore _devices;

        public string BaseUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";

        public DeviceController(IDeviceStore devices) {
            _devices = devices;
        }

        [HttpGet(ApiRoutes.Device.GetAll)]
        public async Task<IActionResult> GetAll() {
            IReadOnlyList<Device> allDevices = await _devices.GetAllAsync();
            return Ok(allDevices);
        }

        [HttpGet(ApiRoutes.Device.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid deviceId) {
            Device device = await _devices.GetByIdAsync(deviceId);

            if (device == null) {
                return NotFound();
            }

            return Ok(device);
        }

        [HttpPost(ApiRoutes.Device.Create)]
        public async Task<IActionResult> Create([FromBody] CreateDeviceRequest deviceRequest) {

            Device device = new() {
                Id = Guid.NewGuid(),
                Alias = deviceRequest.HardwareId,
                OwnerId = HttpContext.GetUserId().ToString(),
            };

            await _devices.CreateAsync(device);

            var locationUri = BaseUrl + "/" + ApiRoutes.Device.Get.Replace("{deviceId}", device.Id.ToString());

            var response = new DeviceResponses { Id = device.Id };
            return Created(locationUri, response);
        }

        [HttpPut(ApiRoutes.Device.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid deviceId, [FromBody] UpdateDeviceRequest request) {

            Guid userId = (Guid)HttpContext.GetUserId();

            Device? device = await _devices.UserOwnsDeviceAsync(userId, deviceId);

            if (device == null) {
                return BadRequest(new { error = "You do not own this device." });
            }

            device.Alias = request.Name;

            bool updated = await _devices.UpdateAsync(device);
            if (!updated) {
                return NotFound();
            }

            return Ok(device);
        }


        [HttpDelete(ApiRoutes.Device.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid deviceId) {

            Guid? n_userId = HttpContext.GetUserId();
            if (n_userId == null) {
                return BadRequest();
            }

            Guid userId = (Guid)n_userId;

            var device = await _devices.UserOwnsDeviceAsync(userId, deviceId);

            if (device == null) {
                return BadRequest(new { error = "You do not own this device." });
            }

            bool deleted = await _devices.DeleteAsync(deviceId);

            if (!deleted) {
                return NotFound();
            }

            return NoContent();
        }

    }

}