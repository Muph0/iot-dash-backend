using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace IotDash.Tests.Adapters {
    class TestCaseScope : IServiceProvider, IServiceScope {
        private readonly bool throwOnUnknown;
        private Dictionary<Type, object> services = new();

        public TestCaseScope(bool throwOnUnknown = false) {
            this.throwOnUnknown = throwOnUnknown;
        }

        public IServiceProvider ServiceProvider => this;
        public void AddService(Type serviceType, object serviceInstance)
            => services.Add(serviceType, serviceInstance);
        public void AddMock<T>() where T : class {
            AddService(typeof(T), Substitute.For<T>());
        }

        public void Dispose() {
            foreach (var service in services.Values) {
                if (service is IDisposable disp) {
                    disp.Dispose();
                }
            }
        }

        public object? GetService(Type serviceType) {
            if (!services.TryGetValue(serviceType, out var instance) && throwOnUnknown) {
                throw new Exception("Service not provided in this test case.");
            }

            return instance;
        }
    }

}

