using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;

namespace IotDash.Settings {

    public class MqttSettings : Settings {
        public class ClientSettings {
            public string Id { set; get; }
        }
        public class CredentialsSettings {
            public string UserName { set; get; }
            public string Password { set; get; }
        }
        public class BrokerHostSettings {
            public string Host { set; get; }
            public int Port { set; get; }
            public int? MaxReconnectionAttempts { get; set; } = 5;
        }

        public ClientSettings Client { get; set; }
        public CredentialsSettings? Credentials { get; set; }
        public BrokerHostSettings Broker { get; set; }

        public static MqttSettings LoadFrom(IConfiguration configuration) {
            return LoadFrom<MqttSettings>(configuration);
        }
    }

}