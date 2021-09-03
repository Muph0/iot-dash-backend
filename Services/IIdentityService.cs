using IotDash.Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace IotDash.Services {
    public interface IIdentityService {
        Task<AuthenticationResult> RegisterAsync(string email, string password);
        Task<AuthenticationResult> LoginAsync(string email, string password);
        Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken);
        Task<GenericResult<int>> CleanupRefreshTokens();
        Task<IdentityUser?> GetUserByIdAsync(Guid userId);
    }
}