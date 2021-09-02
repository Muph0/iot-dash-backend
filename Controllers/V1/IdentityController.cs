using IotDash.Contracts.V1;
using IotDash.Domain;
using IotDash.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;



namespace IotDash.Controllers.V1 {

    public class IdentityController : Controller {

        private readonly IIdentityService _identityService;
        public IdentityController(IIdentityService identityService) {
            _identityService = identityService;
        }

        [HttpPost(ApiRoutes.Identity.Register)]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request) {

            if (!ModelState.IsValid) {
                return BadRequest(new AuthFailResponse {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(err => err.ErrorMessage))
                }); ;
            }

            AuthenticationResult authResult = await _identityService.RegisterAsync(request.Email, request.Password);

            if (!authResult.IsSuccess) {
                return BadRequest(new AuthFailResponse {
                    Errors = authResult.Errors
                });
            }

            return Ok(new AuthSuccessResponse {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken,
            });
        }

        [HttpPost(ApiRoutes.Identity.Login)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request) {

            AuthenticationResult authResult = await _identityService.LoginAsync(request.Email, request.Password);
            return GenerateAuthenticationResponse(authResult);
        }

        [HttpPost(ApiRoutes.Identity.Refresh)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request) {

            AuthenticationResult authResult = await _identityService.RefreshTokenAsync(request.Token, request.RefreshToken);
            return GenerateAuthenticationResponse(authResult);
        }

        private IActionResult GenerateAuthenticationResponse(AuthenticationResult authResult) {
            if (!authResult.IsSuccess) {
                return BadRequest(new AuthFailResponse {
                    Errors = authResult.Errors
                });
            }

            return Ok(new AuthSuccessResponse {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken,
            });
        }

        [HttpGet(ApiRoutes.Identity.GetAllUsers)]
        public async Task<IActionResult> GetAllUsers() {

            return Ok(
                (_identityService as IdentityService)._userManager.Users.Select(u => new { u.Email }).ToList()
            );
        }
    }
}