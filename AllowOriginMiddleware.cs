using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IotDash {
    internal class AllowOriginMiddleware {


        private readonly RequestDelegate _next;
        private readonly ILogger logger;

        public AllowOriginMiddleware(RequestDelegate next, ILogger<AllowOriginMiddleware> logger) {
            _next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) {

            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            await _next(context);

        }

    }
}