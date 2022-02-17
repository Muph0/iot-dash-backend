using IotDash.Data;
using IotDash.Data.Model;
using Microsoft.EntityFrameworkCore;
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

        public async Task RebuildManagers(IServiceProvider provider) {

            var db = provider.GetRequiredService<DataContext>();

            var allDevices = await db.Interfaces.ToListAsync();
            foreach (var device in allDevices) {
                await RebuildManagerFor(device, provider);
            }
        }

        Task RebuildManagerFor(IotInterface iface, IServiceProvider scope);
        Task DiscardManagerFor(IotInterface iface);

    }

}