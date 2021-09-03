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
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidAlgorithms = new[] { jwtSettings.Algorithm },
                ClockSkew = jwtSettings.ClockSkew,
            };

            services.AddSingleton(validationParameters);
            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt => {
                opt.SaveToken = true;
                opt.TokenValidationParameters = validationParameters;
            });//*/

            // configure authorization
            services.AddAuthorization(opt => {
                opt.AddPolicy(nameof(Authorization.Policies.Default), Authorization.Policies.Default);

                var defualtPolicy = opt.GetPolicy(nameof(Authorization.Policies.Default));
                Debug.Assert(defualtPolicy != null);
                opt.DefaultPolicy = defualtPolicy;
            });//*/

            services.AddScoped<IAuthorizationHandler, Authorization.UserExistsHandler>();

            // Add swagger (with JWT bearer support)
            services.AddSwaggerGen(opt => {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "IOT Dash", Version = "v1" });

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