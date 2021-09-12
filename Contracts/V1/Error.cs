using IotDash.Installers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
/*
namespace IotDash.Contracts.V1 {

    public class Error {

        public string Message { get; private set; }
        public int Code { get; private set; }
        public string Name { get; private set; }
        public Error(string name, string message, int code) {
            Message = message;
            Name = name;
        }

        public static Error RequestModel(string message = "Request in wrong format.")
            => new Error(nameof(RequestModel), message, 32);
        public static Error NoSuchUser(string message = "No such user exists.")
            => new Error(nameof(RequestModel), message, 33);

        public static Error Unspecified(string message = "Unspecified error.")
            => new Error(nameof(Unspecified), message, 1);
    }
}
*/