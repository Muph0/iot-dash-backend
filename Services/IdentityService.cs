

using IotDash.Contracts;
using IotDash.Data;
using IotDash.Data.Model;
using IotDash.Domain;
using IotDash.Extensions;
using IotDash.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace IotDash.Services {
    public class IdentityService : IIdentityService {

        internal readonly UserManager<IdentityUser> _userManager;
        internal readonly JwtSettings _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly DataContext _dataContext;

        public IdentityService(UserManager<IdentityUser> userManager,
                                JwtSettings jwtSettings,
                                TokenValidationParameters tokenValidationParameters,
                                DataContext dataContext) {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _tokenValidationParameters = tokenValidationParameters;
            _dataContext = dataContext;
        }

        public async Task<AuthenticationResult> LoginAsync(string email, string password) {

            var user = await _userManager.FindByEmailAsync(email);

            AuthenticationResult failBadPassword = AuthenticationResult.Fail("User/password combination is wrong.");

            // if user dont exist
            if (user == null
            // or if bad password
            || !await _userManager.CheckPasswordAsync(user, password)) {
                return failBadPassword;
            }

            return await GenerateTokenPairForUserAsync(user);
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken) {

            var (validatedToken, errors) = GetPrincipalFromExpiredToken(token);

            if (validatedToken == null) {
                return AuthenticationResult.Fail(errors);
            }

            var expiryDateUtc = validatedToken.GetExpiryDate();

            // JWT must be expired
            if (expiryDateUtc > DateTime.UtcNow) {
                return AuthenticationResult.Fail($"This token has not expired yet. {expiryDateUtc - DateTime.UtcNow} remaining.");
            }

            var jti = validatedToken.GetClaim(JwtRegisteredClaimNames.Jti);
            var storedRefreshToken = await _dataContext.RefreshTokens.SingleOrDefaultAsync(t => t.Token == refreshToken);

            if (storedRefreshToken == null) {
                return AuthenticationResult.Fail("This refresh token does not exist.");
            }

            // RefreshToken must not be expired
            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate) {
                return AuthenticationResult.Fail("Refresh token is expired.");
            }

            if (storedRefreshToken.Used) {
                return AuthenticationResult.Fail("Refresh token has been used.");
            }

            if (jti != storedRefreshToken.JwtId) {
                return AuthenticationResult.Fail("Refresh token does not match this JWT.");
            }

            storedRefreshToken.Used = true;
            _dataContext.RefreshTokens.Update(storedRefreshToken);
            await _dataContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(validatedToken.GetClaim(JwtCustomClaimNames.Id));
            return await GenerateTokenPairForUserAsync(user);
        }

        public async Task<AuthenticationResult> RegisterAsync(string email, string password) {

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null) {
                return AuthenticationResult.Fail("User with this email already exists.");
            }

            var newUser = new IdentityUser {
                Email = email,
                UserName = email,
            };

            var createUser = await _userManager.CreateAsync(newUser, password);
            if (!createUser.Succeeded) {
                return AuthenticationResult.Fail(createUser.Errors.Select(err => $"{err.Description}"));
            }

            return await GenerateTokenPairForUserAsync(newUser);
        }

        public async Task<GenericResult<int>> CleanupRefreshTokens() {

            int deleted = await _dataContext.RefreshTokens.Where(t => t.Invalidated || t.Used || t.ExpiryDate > DateTime.UtcNow).DeleteAsync();
            await _dataContext.SaveChangesAsync();

            return GenericResult<int>.Ok(deleted);
        }


        private (ClaimsPrincipal, IEnumerable<string>) GetPrincipalFromExpiredToken(string jwt) {

            var tokenHandler = new JwtSecurityTokenHandler();
            try {
                var parameters = _tokenValidationParameters.Clone();
                parameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(jwt, parameters, out var validatedToken);
                return (principal, null);
            } catch (SecurityTokenException ex) {
                return (null, ex.Data.Keys.Cast<string>().Prepend("Invalid token."));
            }
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken) {
            return (validatedToken is JwtSecurityToken validJwt) &&
                validJwt.Header.Alg.Equals(_jwtSettings.Algorithm, StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task<AuthenticationResult> GenerateTokenPairForUserAsync(IdentityUser user) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtCustomClaimNames.Id, user.Id)
                }),
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = new RefreshToken {
                Token = Guid.NewGuid().ToString(),
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.Add(_jwtSettings.RefreshTokenLifetime)
            };

            await _dataContext.RefreshTokens.AddAsync(refreshToken);
            await _dataContext.SaveChangesAsync();

            return AuthenticationResult.Success(tokenHandler.WriteToken(token), refreshToken.Token);
        }

        public async Task<IdentityUser?> GetUserByIdAsync(Guid userId) {
            return await _dataContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId.ToString());
        }
    }
}