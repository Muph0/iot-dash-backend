using IotDash.Data;
using IotDash.Data.Model;
using IotDash.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Services {

    internal class ExpressionManager : IHostedExpressionManager {

        private readonly IHostedMqttClient mqtt;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger logger;
        private readonly Dictionary<(Guid, int), ManagedInterface> managedInterfaces = new();

        public ExpressionManager(IServiceScopeFactory scopeFactory, ILogger<ExpressionManager> logger) {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken) {
            logger.LogDebug($"Rebuilding all interface managers.");
            await RefreshAllDevices();
        }

        public async Task RefreshAllDevices() {
            using (var scope = scopeFactory.CreateScope()) {
                await RefreshAllDevices(scope.ServiceProvider);
            }
        }

        public async Task RefreshAllDevices(IServiceProvider provider) {

            var db = provider.GetRequiredService<DataContext>();

            var allDevices = db.Devices.ToList();
            foreach (var device in allDevices) {
                await RefreshDevice(provider, device);
            }
        }

        public async Task RefreshDevice(IServiceProvider provider, IotDevice device) {
            foreach (var iface in device.Interfaces) {
                await RefreshInterface(provider, iface);
            }
        }

        public async Task RefreshInterface(IServiceProvider provider, IotInterface iface) {

            if (iface.Kind == Contracts.V1.InterfaceKind.Switch) {
                var key = (iface.DeviceId, iface.Id);

                if (managedInterfaces.TryGetValue(key, out var mgr)) {
                    mgr.Dispose();
                    managedInterfaces.Remove(key);
                }

                if (iface.Expression != null) {
                    logger.LogTrace($"Rebuilding manager of interface {{{iface}}}.");
                    var managedIface = await ManagedInterface.Create(provider, iface);
                    managedInterfaces.Add(key, managedIface);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

    }

}