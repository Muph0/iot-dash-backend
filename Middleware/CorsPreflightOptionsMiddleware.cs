using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace IotDash {
    public class CorsPreflightOptionsMiddleware {
        private readonly RequestDelegate _next;

        public CorsPreflightOptionsMiddleware(RequestDelegate next) {
            _next = next;
        }

        public Task Invoke(HttpContext context) {
            return BeginInvoke(context);
        }

        private Task BeginInvoke(HttpContext context) {
            context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            if (context.Request.Method == "OPTIONS") {
                context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept, Authorization" });
                context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS, PATCH" });
                context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
                context.Response.StatusCode = 200;
                return context.Response.WriteAsync("OK");
            }

            return _next.Invoke(context);
        }
    }

    public static class OptionsMiddlewareExtensions {
        public static IApplicationBuilder UseCorsOptions(this IApplicationBuilder builder) {
            return builder.UseMiddleware<CorsPreflightOptionsMiddleware>();
        }
    }
}