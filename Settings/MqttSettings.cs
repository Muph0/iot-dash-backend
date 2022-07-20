namespace IotDash.Settings {

    public class MqttSettings : Settings<MqttSettings> {
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
            public int? MaxReconnectionAttempts { get; set; } = -1;

            public bool ExceededMaxAttempts(int reconnectionAttempts)
                => reconnectionAttempts >= MaxReconnectionAttempts && MaxReconnectionAttempts > 0;
        }

        public ClientSettings Client { get; set; }
        public CredentialsSettings? Credentials { get; set; }
        public BrokerHostSettings Broker { get; set; }
    }

}