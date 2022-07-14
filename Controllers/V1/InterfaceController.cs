using IotDash.Authorization;
using IotDash.Contracts;
using IotDash.Contracts.V1;
using IotDash.Contracts.V1.Model.Extensions;
using IotDash.Data.Model;
using IotDash.Utils.Context;
using IotDash.Utils.Error;
using IotDash.Utils.ObjectMapping;
using IotDash.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Controllers.V1 {


    public class InterfaceController : Controller {

        private readonly IInterfaceStore interfaces;
        private readonly IIdentityService identity;
        private readonly IUserStore users;
        private readonly IHistoryStore history;

        public string BaseUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";

        public InterfaceController(IIdentityService identity, IInterfaceStore interfaces,
                 IUserStore users, IHistoryStore history) {
            this.interfaces = interfaces;
            this.identity = identity;
            this.users = users;
            this.history = history;
        }

        /// <summary>
        /// Get information about all interfaces.
        /// </summary>
        /// <returns>The interface information array.</returns>
        [Authorize(Policy = nameof(Policies.Authorized))]
        [HttpGet(ApiRoutes.Interface.Base)]
        [Produces(MimeType.Application_JSON, Type = typeof(IList<Contracts.V1.Model.IotInterface>))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetInterfaces() {
            var domain = await this.interfaces.GetAllAsync();
            return Ok(domain.Select(i => i.ToContract()));
        }

        /// <summary>
        /// Get information about the specific interface.
        /// </summary>
        /// <returns>The interface information.</returns>
        [Authorize(Policy = nameof(Policies.AuthorizedInterfaceAccess))]
        [HttpGet(ApiRoutes.Interface.Get)]
        [Produces(MimeType.Application_JSON, Type = typeof(Contracts.V1.Model.IotInterface))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public Task<IActionResult> GetInterface() {

            var iface = HttpContext.Features.Get<IotInterface>();
            return Task.FromResult<IActionResult>(
                Ok(iface.ToContract())
            );
        }

        /// <summary>
        /// Create a new interface.
        /// </summary>
        /// <param name="request">Interface creation request.</param>
        /// <returns>The created interface and Location header with the interface uri.</returns>
        [Authorize(Policy = nameof(Policies.Authorized))]
        [HttpPost(ApiRoutes.Interface.Create)]
        [Produces(MimeType.Application_JSON)]
        [ProducesResponseType(201, Type = typeof(InterfaceResponse))]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateInterface([FromBody] InterfaceCreateRequest request) {

            if (!ModelState.IsValid) {
                return InterfaceResponse.BadRequest(ModelState.ErrorMessages());
            }

            var user = HttpContext.Features.GetRequired<IdentityUser>();
            Guid ifaceId = new();

            IotInterface newIface = request.CreateModel(ifaceId, user);
            await interfaces.CreateAsync(newIface);
            await interfaces.SaveChangesAsync();

            return InterfaceResponse.Created(ApiRoutes.Interface.GetUri(newIface), newIface.ToContract());
        }

        /// <summary>
        /// Update one or more fields of the specified interface.
        /// </summary>
        /// <param name="request">Update request. At least one field must be specified.</param>
        /// <returns>The updated interface.</returns>
        [Authorize(Policy = nameof(Policies.AuthorizedInterfaceAccess))]
        [HttpPatch(ApiRoutes.Interface.Update)]
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

        /// <summary>
        /// Delete a specific interface.
        /// </summary>
        /// <returns>No content.</returns>
        [Authorize(Policy = nameof(Policies.AuthorizedInterfaceAccess))]
        [HttpDelete(ApiRoutes.Interface.Delete)]
        [Produces(MimeType.Application_JSON, Type = typeof(InterfaceResponse))]
        public async Task<IActionResult> DeleteInterface() {

            var iface = HttpContext.Features.GetRequired<IotInterface>();

            bool ok = await interfaces.DeleteByKeyAsync(iface.Id);
            await interfaces.SaveChangesAsync();

            return ok
                ? InterfaceResponse.Ok(iface.ToContract())
                : InterfaceResponse.NotFound(Error.DeviceAlreadyDeleted());
        }

        /// <summary>
        /// Get interface history over a specified time period with given point density.
        /// </summary>
        /// <param name="request">Time period information with point density.</param>
        /// <returns>List of data points.</returns>
        [Authorize(Policy = nameof(Policies.AuthorizedInterfaceAccess))]
        [HttpPost(ApiRoutes.Interface.History)]
        [Produces(MimeType.Application_JSON, Type = typeof(HistoryResponse))]
        public async Task<IActionResult> GetHistory([FromBody] HistoryRequest request) {

            if (!ModelState.IsValid) {
                return HistoryResponse.BadRequest(ModelState.ErrorMessages());
            }

            var iface = HttpContext.Features.GetRequired<IotInterface>();
            var results = await history.GetPagedHistoryAsync(iface, request);

            return HistoryResponse.Ok(results.Select(r => r.ToContract()));
        }
    }
}