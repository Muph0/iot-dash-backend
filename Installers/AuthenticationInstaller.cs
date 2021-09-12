using IotDash.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace IotDash.Installers {

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

        }
    }

}