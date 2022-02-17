using IotDash.Contracts;
using IotDash.Contracts.V1;
using IotDash.Domain;
using IotDash.Utils;
using IotDash.Utils.Context;
using IotDash.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace IotDash.Controllers.V1 {

    public class IdentityController : Controller {

        private readonly IIdentityService identity;
        private readonly IUserStore users;

        public IdentityController(IIdentityService identity, IUserStore users) {
            this.identity = identity;
            this.users = users;
        }

        /// <summary>
        /// Register a new user with the given credentials.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Identity.Register)]
        [Produces(MimeType.Application_JSON, Type = typeof(AuthResponse))]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request) {

            if (!ModelState.IsValid) {
                return AuthResponse.BadRequest(ModelState.Values.SelectMany(v => v.Errors.Select(err => err.ErrorMessage)));
            }

            var authResult = await identity.RegisterAsync(request.Email, request.Password);
            return authResult.AsOkOrBadRequest();
        }

        /// <summary>
        /// Get a new authorization token for the user.
        /// </summary>
        /// <param name="request">Credentials of the user to log-in.</param>
        /// <returns>New token pair wrapped in <see cref="AuthResponse"/>.</returns>
        [HttpPost(ApiRoutes.Identity.Login)]
        [Produces(MimeType.Application_JSON, Type = typeof(AuthResponse))]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request) {

            var authResult = await identity.LoginAsync(request.Email, request.Password);
            return authResult.AsOkOrBadRequest();
        }

        /// <summary>
        /// Provide a new token pair in exchange for a valid refresh token and an expired JWT token.
        /// </summary>
        /// <param name="request">Token pair consisting of a valid refresh token and an expired JWT token.</param>
        /// <returns>New token pair wrapped in <see cref="AuthResponse"/>.</returns>
        [HttpPost(ApiRoutes.Identity.Refresh)]
        [Produces(MimeType.Application_JSON, Type = typeof(AuthResponse))]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request) {

            var authResult = await identity.RefreshTokenAsync(request.Token, request.RefreshToken);
            return authResult.AsOkOrBadRequest();
        }

        /// <summary>
        /// Get information about the authorization token bearer.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet(ApiRoutes.Identity.Me)]
        [Produces(MimeType.Application_JSON, Type = typeof(Contracts.V1.Model.User))]
        public Task<IActionResult> GetAllUsers() {

            ClaimsPrincipal token = HttpContext.User;
            var user = HttpContext.Features.GetRequired<IdentityUser>();

            return Task.FromResult<IActionResult>(Ok(user.ToContract()));
        }
    }
}