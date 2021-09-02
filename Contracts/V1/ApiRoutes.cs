using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Contracts.V1 {

    public static class ApiRoutes {

        public const string Root = "api";
        public const string Version = "v1";

        public const string Base = Root + "/" + Version;

        public static class Device {
            public const string deviceId = "{deviceId}";
            public const string Base = ApiRoutes.Base + "/device";

            public const string GetAll = Base + "s";
            public const string Get = Base + "/" + deviceId;
            public const string Create = Base;
            public const string Update = Base + "/" + deviceId;
            public const string Delete = Base + "/" + deviceId;

            public static class Interface {
                public const string ifaceId = "{ifaceId}";
                public const string Base = Device.Base + "/interface";

                public const string GetAll = Base + "s";
                public const string Get = Base + "/" + ifaceId;
                public const string ReadValue = Get + "/value";
                
            }
        }

        public static class Identity {
            public const string Base = ApiRoutes.Base + "/identity";

            public const string Login = Base + "/login";
            public const string Register = Base + "/register";
            public const string Refresh = Base + "/refresh";
            public const string GetAllUsers = Base + "/users";
        }

    }
}
