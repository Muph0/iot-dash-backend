using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Settings {

    public class MqttSettings : Settings {

        public string BrokerAddress { get; set; }

        public static MqttSettings LoadFrom(IConfiguration configuration) {
            return LoadFrom<MqttSettings>(configuration);
        }
    }

}