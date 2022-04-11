using IotDash.Contracts;
using IotDash.Contracts.V1;
using IotDash.Utils.Context;
using IotDash.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IotDash.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace IotDash.Controllers.V1 {
    public class ChartHub : Hub {
        public const string MethodNewData = "newdata";

        public ChartHub() {

        }

    }
}