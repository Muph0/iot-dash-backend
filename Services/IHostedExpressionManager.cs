using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IotDash.Services {

    public interface IHostedExpressionManager : IHostedService {

        Task RefreshInterface(IServiceProvider scope, Data.Model.IotInterface iface);

        Task RefreshDevice(IServiceProvider scope, Data.Model.IotDevice device);

        Task RefreshAllDevices(IServiceProvider scope);

    }

}