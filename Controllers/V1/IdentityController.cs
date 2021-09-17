using IotDash.Contracts;
using IotDash.Contracts.V1;
using IotDash.Domain;
using IotDash.Extensions;
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

        [HttpPost(ApiRoutes.Identity.Register)]
        [Produces(MimeType.Application_JSON, Type = typeof(AuthResponse))]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request) {

            if (!ModelState.IsValid) {
                return AuthResponse.BadRequest(ModelState.Values.SelectMany(v => v.Errors.Select(err => err.ErrorMessage)));
            }

            var authResult = await identity.RegisterAsync(request.Email, request.Password);
            return authResult.AsOkOrBadRequest();
        }

        [HttpPost(ApiRoutes.Identity.Login)]
        [Produces(MimeType.Application_JSON, Type = typeof(AuthResponse))]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request) {

            var authResult = await identity.LoginAsync(request.Email, request.Password);
            return authResult.AsOkOrBadRequest();
        }

        [HttpPost(ApiRoutes.Identity.Refresh)]
        [Produces(MimeType.Application_JSON, Type = typeof(AuthResponse))]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request) {

            var authResult = await identity.RefreshTokenAsync(request.Token, request.RefreshToken);
            return authResult.AsOkOrBadRequest();
        }

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