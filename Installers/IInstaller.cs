using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IotDash.Installers {

    public interface IInstaller {
        void InstallServices(IServiceCollection services, IConfiguration configuration);
    }

}