using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;

namespace IotDash.Settings {

    public class JwtSettings : Settings {
        public string Secret { get; set; }
        public TimeSpan TokenLifetime { get; set; }
        public TimeSpan RefreshTokenLifetime { get; set; }
        public string ValidAlgorithm { get; set; }

        public static JwtSettings LoadFrom(IConfiguration configuration) {
            return LoadFrom<JwtSettings>(configuration);
        }
    }

}