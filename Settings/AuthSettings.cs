namespace IotDash.Settings {

    /// <summary>
    /// Settings for REST authentification.
    /// The username and password which are used to authentcate over HTTP API.
    /// </summary>
    public class AuthSettings : Settings<AuthSettings> {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}