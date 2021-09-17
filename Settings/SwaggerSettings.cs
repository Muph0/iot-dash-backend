using Microsoft.Extensions.Configuration;
using System;

namespace IotDash.Settings {

    public class SwaggerSettings : Settings {
        public string JsonRoute { get; set; }
        public string Description { get; set; }
        public string UiEndpoint { get; set; }
        public string Server { get; set; }

        public static SwaggerSettings Load(IConfiguration configuration) {
            return LoadFrom<SwaggerSettings>(configuration);
        }
    }

}