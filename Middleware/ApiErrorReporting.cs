using IotDash.Exceptions;
using IotDash.Extensions;
using IotDash.Extensions.ObjectMapping;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IotDash {
    public class ApiErrorReporting {

        private readonly RequestDelegate _next;
        private readonly ILogger logger;
        private readonly IWebHostEnvironment env;

        public ApiErrorReporting(RequestDelegate next, ILogger<ApiErrorReporting> logger, IWebHostEnvironment env) {
            _next = next;
            this.logger = logger;
            this.env = env;
        }

        public async Task InvokeAsync(HttpContext context) {
            
            ApplicationException exception;
            try {
                
                await _next(context);
                exception = context.Features.Get<ApplicationException>();

            } catch (ApplicationException ex) {
                exception = ex;
            }

            if (exception != null)
                await RespondToException(context, exception);
        }

        public async Task RespondToException(HttpContext context, ApplicationException exception) {

            logger.LogError(exception, "Received exception.");

            if (exception is BadRequestException) {
                context.Response.StatusCode = 400;
            } else if (exception is UnauthorizedException) {
                context.Response.StatusCode = 401;
            } else if (exception is OperationForbiddenException) {
                context.Response.StatusCode = 403;
            } else if (exception is ResourceNotFoundException) {
                context.Response.StatusCode = 404;
            }

            var quotedErrors = exception.RecursiveEnumerate<Exception>(ex => ex.InnerException).Select(ex => '"' + ex.Message + '"');

            if (env.IsDevelopment()) {
                await context.Response.WriteAsync($"{{\"success\":false, \"errors\": [{(string.Join(',', quotedErrors))}]}}");
            } else {
                await context.Response.WriteAsync($"{{\"success\":false, \"errors\": [\"{exception.GetType().Name.Replace("Exception", "")}\"]}}");
            }
        }
    }
}
