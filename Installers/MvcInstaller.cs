using IotDash.Domain;
using IotDash.Extensions;
using IotDash.Services;
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
using System.Linq;
using System.Text;

namespace IotDash.Installers {

    internal class MvcInstaller : IInstaller {

        private delegate void PolicyBuilder(AuthorizationPolicyBuilder builder);

        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            // Register controllers
            services.AddControllers(opt => {
            });

            SwaggerSettings swagg = SwaggerSettings.LoadFrom(configuration);

            // Add swagger (with JWT bearer support)
            services.AddSwaggerGen(opt => {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "IOT Dash", Version = "v1" });

                opt.AddServer(new OpenApiServer {
                    Url = swagg.Server
                });

                // configure swagger for JWT
                var bearerScheme = new OpenApiSecurityScheme {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the bearer scheme.\r\n" +
                    "Enter 'Bearer' [space] and then your valid token in the text input below.",
                };

                var security = new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme() {
                            Reference = new() {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new string[0]
                    }
                };

                opt.AddSecurityDefinition("Bearer", bearerScheme);
                opt.AddSecurityRequirement(security);
            });
        }
    }

}