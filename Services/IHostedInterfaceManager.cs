using IotDash.Data;
using IotDash.Data.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash.Services {

    public interface IHostedInterfaceManager : IHostedService {

        public async Task RebuildManagersForAllDevices(IServiceProvider provider) {

            var db = provider.GetRequiredService<DataContext>();

            var allDevices = db.Devices.ToList();
            foreach (var device in allDevices) {
                await RebuildManagerFor(device, provider);
            }
        }

        public async Task RebuildManagerFor(IotDevice device, IServiceProvider provider) {
            foreach (var iface in device.Interfaces) {
                await RebuildManagerFor(iface, provider);
            }
        }

        Task RebuildManagerFor(IotInterface iface, IServiceProvider scope);

        Task DiscardManagerFor(IotInterface iface);

    }

}