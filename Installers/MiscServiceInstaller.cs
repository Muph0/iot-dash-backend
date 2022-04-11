using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IotDash.Services;
using IotDash.Services.Messaging;
using IotDash.Services.Messaging.Implementation;
using IotDash.Services.Evaluation;
using IotDash.Services.History;
using IotDash.Services.Domain;

namespace IotDash.Installers {

    internal class MiscServiceInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            // Message mediator
            services.AddSingleton<IMessageMediator, MessageMediator>();

            // Add all entity managers
            services.AddAllImplementationsInAssembly<IEntityManagementService>(ServiceLifetime.Singleton);

            // SignalR
            services.AddSignalR();
        }
    }
}