using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using System;
using SignalRSwaggerGen.Attributes;
using System.Reflection;
using System.Linq;

namespace IotDash.Utils {
    public static class SignalRExtensions {
        public static HubEndpointConventionBuilder MapHub<THub>(this IEndpointRouteBuilder endpoints) where THub : Hub {
            var attributes = typeof(THub).GetCustomAttributes<SignalRHubAttribute>(true);
            if (attributes.Count() != 1)
                throw new ArgumentException($"Hub has no {typeof(SignalRHubAttribute).FullName} attributes");

            var attribute = attributes.Single();
            return endpoints.MapHub<THub>(attribute.Path);
        }
    }
}
