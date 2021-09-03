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

    public class MqttInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            MqttSettings settings = MqttSettings.LoadFrom(configuration);


        }
    }

}