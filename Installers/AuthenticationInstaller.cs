using IotDash.Services.Auth;
using IotDash.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IotDash.Installers {

    /// <summary>
    /// Configures and registers the <see cref="Microsoft.AspNetCore.Authentication.IAuthenticationService"/>.
    /// </summary>
    internal class AuthenticationInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            // Setup JWT middleware
            var jwtSettings = JwtSettings.LoadFrom(configuration);
            services.AddSingleton(jwtSettings);

            TokenValidationParameters validationParameters = new() {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidAlgorithms = new[] { jwtSettings.Algorithm },
                ClockSkew = jwtSettings.ClockSkew,
            };

            // Register validation parameters as service
            services.AddSingleton(validationParameters);

            // register ASP.NET authentication services
            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt => {
                opt.SaveToken = true;
                opt.TokenValidationParameters = validationParameters;
            });

            // Add auth settings
            var authSettings = Settings.AuthSettings.LoadFrom(configuration);
            services.AddSingleton(authSettings);

            // Add db preparation service
            services.AddSingleton<HostedAuthPreparer>();
            services.AddHostedService<HostedAuthPreparer>();
        }
    }

}