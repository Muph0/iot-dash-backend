using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IotDash.Services;
using IotDash.Services.Messaging;
using IotDash.Services.Messaging.Implementation;
using IotDash.Services.Evaluation;
using IotDash.Services.History;
using IotDash.Services.Domain;

namespace IotDash.Installers {
    internal class EntityManagersInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            // Add all entity managers
            services.AddAllImplementationsInAssembly<IEntityManagementService>(ServiceLifetime.Singleton);

        }
    }

    internal class MiscServiceInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            ////////////////
            // Message mediator
            services.AddSingleton<IMessageMediator, MessageMediator>();

            ////////////////
            // Evaluator service
            // old
            //services.AddSingleton<IHostedEvaluationService, EvaluatorsManager>();
            //services.AddHostedService(p => p.GetRequiredService<IHostedEvaluationService>());

            ////////////////
            // History logger service
            //// old
            //services.AddSingleton<IHostedHistoryService, HistoryService0>();
            //services.AddHostedService(p => p.GetRequiredService<IHostedHistoryService>());
            // new
            //services.AddSingleton<HostedHistoryService>();
            //services.AddHostedService(p => p.GetRequiredService<HostedHistoryService>());
        }
    }
}