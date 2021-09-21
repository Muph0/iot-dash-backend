using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Settings {

    public class JwtSettings : Settings<JwtSettings> {

        public string Secret { get; set; }
        public TimeSpan TokenLifetime { get; set; }
        public TimeSpan RefreshTokenLifetime { get; set; }
        public TimeSpan ClockSkew { get; set; }
        public string Algorithm { get; set; }
    }

}