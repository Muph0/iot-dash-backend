using System.Diagnostics;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using IotDash.Authorization.Requirements;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IotDash.Installers {

    internal class AuthorizationInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {

            // Register authorization handlers
            services.AddAllImplementationsInAssembly<IAuthorizationHandler>();

            // configure authorization
            services.AddAuthorization(opt => {

                var policies = typeof(Authorization.Policies).GetMethods().Where(m => {
                    var parameters = m.GetParameters();
                    return m.IsStatic && m.ReturnType == typeof(void) && parameters.Length == 1 && !parameters[0].IsOut
                        && parameters[0].ParameterType == typeof(AuthorizationPolicyBuilder);
                });

                Debug.Assert(policies.Count() != 0);

                foreach (var m in policies) {
                    opt.AddPolicy(m.Name, (builder) => m.Invoke(null, new object[] { builder }));
                }

                // check that we have handler for each requirement
                var missingHandlers = policies
                    .SelectMany(m => opt.GetPolicy(m.Name).Requirements)
                    .Select(req => req.GetType())
                    .Distinct()
                    .Where(req => {
                        var handlerType = typeof(AuthorizationHandler<>).MakeGenericType(req);
                        return !services.Any(s =>
                            s.ServiceType == typeof(IAuthorizationHandler)
                            && s.ImplementationType.IsAssignableTo(handlerType)
                            );
                    }).ToArray();

                Debug.Assert(missingHandlers.Count() == 0);

                var defualtPolicy = opt.GetPolicy(nameof(Authorization.Policies.Authorized));
                Debug.Assert(defualtPolicy != null);
                opt.DefaultPolicy = defualtPolicy;
                opt.InvokeHandlersAfterFailure = false;
            });

            //services.Replace<IAuthorizationEvaluator, Services.Implementations.AuthEvaluator>();
            services.Replace<IAuthorizationService, Services.Auth.AuthService>();
        }
    }


}