

using IotDash.Contracts;
using IotDash.Contracts.V1;
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
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace IotDash.Services.Implementations {
    internal class IdentityService : IIdentityService {
        //
        private readonly IUserStore users;
        internal readonly JwtSettings jwtSettings;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly DataContext db;

        public IdentityService(IUserStore users,
                                JwtSettings jwtSettings,
                                TokenValidationParameters tokenValidationParameters,
                                DataContext dataContext) {
            this.users = users;
            this.jwtSettings = jwtSettings;
            this.tokenValidationParameters = tokenValidationParameters;
            this.db = dataContext;
        }

        public async Task<AuthResponse> LoginAsync(string email, string password) {

            var user = await users.GetByEmailAsync(email);

            var failBadPassword = AuthResponse.Fail(Error.BadUserPasswordCombo());

            // if user dont exist
            if (user == null
            // or if bad password
            || !await users.CheckPasswordAsync(user, password)) {
                return failBadPassword;
            }

            return await GenerateTokenPairForUserAsync(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken) {

            var (validatedToken, errors) = GetPrincipalFromExpiredToken(token);

            if (validatedToken == null) {
                return AuthResponse.Fail(errors);
            }

            var expiryDateUtc = validatedToken.GetExpiryDate();

            // JWT must be expired
            if (expiryDateUtc > DateTime.UtcNow) {
                return AuthResponse.Fail(Error.JwtIsNotExpired((DateTime)expiryDateUtc));
            }

            var jti = validatedToken.GetClaim(JwtRegisteredClaimNames.Jti);
            var storedRefreshToken = await db.RefreshTokens.SingleOrDefaultAsync(t => t.Token == refreshToken);

            if (storedRefreshToken == null) {
                return AuthResponse.Fail(Error.RefreshTokenNonExistent());
            }

            // RefreshToken must not be expired
            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate) {
                return AuthResponse.Fail(Error.RefreshTokenExpired());
            }

            if (storedRefreshToken.Used) {
                return AuthResponse.Fail(Error.RefreshTokenUsedAlready());
            }

            if (jti != storedRefreshToken.JwtId) {
                return AuthResponse.Fail(Error.RefreshTokenJwtMismatch());
            }

            storedRefreshToken.Used = true;
            db.RefreshTokens.Update(storedRefreshToken);
            await db.SaveChangesAsync();

            var userId = validatedToken.GetClaim(JwtCustomClaimNames.Id);
            Debug.Assert(userId != null);

            var user = await users.GetByKeyAsync(userId);
            Debug.Assert(user != null);
            return await GenerateTokenPairForUserAsync(user);
        }

        public async Task<AuthResponse> RegisterAsync(string email, string password) {

            var existingUser = await users.GetByEmailAsync(email);

            if (existingUser != null) {
                return AuthResponse.Fail(Error.EmailTaken());
            }

            var newUser = new IdentityUser {
                Email = email,
                UserName = email,
            };

            var createUser = await users.CreateAsync(newUser, password);
            if (!createUser.Succeeded) {
                return AuthResponse.Fail(createUser.Errors.Select(err => $"{err.Description}"));
            }

            return await GenerateTokenPairForUserAsync(newUser);
        }

        public async Task<int> CleanupRefreshTokens() {

            int deleted = await db.RefreshTokens.Where(t => t.Invalidated || t.Used || t.ExpiryDate > DateTime.UtcNow).DeleteAsync();
            await db.SaveChangesAsync();

            return deleted;
        }

        private (ClaimsPrincipal, IEnumerable<string>) GetPrincipalFromExpiredToken(string jwt) {

            var tokenHandler = new JwtSecurityTokenHandler();
            try {
                var parameters = tokenValidationParameters.Clone();
                parameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(jwt, parameters, out var validatedToken);
                return (principal, null);
            } catch (SecurityTokenException ex) {
                return (null, ex.Data.Keys.Cast<string>().Prepend("Invalid token."));
            }
        }
        private async Task<AuthResponse> GenerateTokenPairForUserAsync(IdentityUser user) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtCustomClaimNames.Id, user.Id)
                }),
                Expires = DateTime.UtcNow.Add(jwtSettings.TokenLifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = new RefreshToken {
                Token = Guid.NewGuid().ToString(),
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.Add(jwtSettings.RefreshTokenLifetime)
            };

            await db.RefreshTokens.AddAsync(refreshToken);
            await db.SaveChangesAsync();

            return AuthResponse.Succeed((tokenHandler.WriteToken(token), refreshToken.Token));
        }

        public async Task<IdentityUser?> GetUserByIdAsync(Guid userId) {
            return await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId.ToString());
        }

        public async Task<IdentityUser?> GetUserByEmailAsync(string email) {
            return await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email);
        }

        
    }
}

