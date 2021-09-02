using IotDash.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IotDash.Installers {

    public class MvcInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            // Register MVC
            services.AddControllers();

            //* Setup JWT middleware
            var jwtSettings = JwtSettings.LoadFrom(configuration);
            services.AddSingleton(jwtSettings);

            TokenValidationParameters validationParameters = new() {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ValidateLifetime = true,
                ValidAlgorithms = new[] { jwtSettings.ValidAlgorithm }
            };

            services.AddSingleton(validationParameters);
            services.AddAuthentication(conf => {
                conf.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                conf.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                conf.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(conf => {
                conf.SaveToken = true;
                conf.TokenValidationParameters = validationParameters;
            });//*/

            // Add swagger
            services.AddSwaggerGen(conf => {
                conf.SwaggerDoc("v1", new OpenApiInfo { Title = "IOT Dash", Version = "v1" });

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

                conf.AddSecurityDefinition("Bearer", bearerScheme);
                conf.AddSecurityRequirement(security);
            });
        }
    }

}