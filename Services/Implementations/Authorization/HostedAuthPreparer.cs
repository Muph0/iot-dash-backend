using IotDash.Data;
using IotDash.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services.Auth {

    /// <summary>
    /// Makes sure that exactly one user account is available in the database
    /// with the correct credentials from <see cref="AuthSettings"/>.
    /// </summary>
    internal class HostedAuthPreparer : IHostedService {
        private readonly AuthSettings settings;
        private readonly IServiceProvider scope;
        private readonly DataContext db;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IIdentityService identityService;

        public HostedAuthPreparer(
            AuthSettings settings,
            IServiceScopeFactory factory
            ) {
            this.settings = settings;
            this.scope = factory.CreateScope().ServiceProvider;
            db = scope.GetRequiredService<DataContext>();
            userManager = scope.GetRequiredService<UserManager<IdentityUser>>();
            identityService = scope.GetRequiredService<IIdentityService>();
        }


        private async Task PrepareSingularUserAccess(AuthSettings settings) {
            var user = db.Users.OrderBy(u => u.Id).FirstOrDefault();

            if (user == null) {
                await db.Users.DeleteFromQueryAsync();
                var result = await identityService.RegisterAsync(settings.Username, settings.Password);
                if (!result.Success) {
                    Debug.Assert(result.Errors != null);
                    throw new Exception($"Couldn't create user: {string.Join(' ', result.Errors)}");
                }
            } else {
                user.Email = "";
                user.NormalizedEmail = "";
                user.UserName = settings.Username;
                user.NormalizedUserName = settings.Username.ToUpper();
                user.PasswordHash = userManager.PasswordHasher.HashPassword(user, settings.Password);
                await db.Users.Where(u => u.Id != user.Id).DeleteFromQueryAsync();
            }

            await db.SaveChangesAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            await PrepareSingularUserAccess(settings);
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }
    }

}