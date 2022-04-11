using IotDash.Data.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Contracts.V1 {

    /// <summary>
    /// Static collection of all http routes.
    /// </summary>
    public static class ApiRoutes {

        public const string Root = "api";
        public const string Version = "v1";

        public const string Base = Root + "/" + Version;


        public static class Interface {
            public const string ifaceId = "{" + nameof(ifaceId) + ":Guid}";
            public const string Base = ApiRoutes.Base + "/interface";

            public const string Get = Base + "/" + ifaceId;
            public const string Update = Get;
            public const string Create = Base;
            public const string Delete = Get;
            public const string Data = Get + "/datastream";
            public const string History = Get + "/history";

            internal static string GetUri(IotInterface newIface) {
                return Get.Replace(ifaceId, newIface.Id.ToString());
            }
        }


        public static class Identity {
            public const string Base = ApiRoutes.Base + "/identity";

            public const string Me = Base;
            public const string Login = Base + "/login";
            public const string Register = Base + "/register";
            public const string Refresh = Base + "/refresh";
        }

    }
}
