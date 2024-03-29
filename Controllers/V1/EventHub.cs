﻿using IotDash.Contracts;
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
using SignalRSwaggerGen.Attributes;

namespace IotDash.Controllers.V1 {

    [SignalRHub(ApiRoutes.Events)]
    public class EventHub : Hub {
        public const string MethodNewData = "newdata";

        public EventHub() {

        }

    }
}