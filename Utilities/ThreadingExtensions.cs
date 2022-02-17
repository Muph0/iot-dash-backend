using System;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Utils.Threading {

    internal static class ThreadingExtensions {

        public static async Task<IDisposable> LockAsync(this SemaphoreSlim semaphore) {
            var guard = new SemGuard(semaphore);
            await semaphore.WaitAsync();
            return guard;
        }

        private sealed class SemGuard : IDisposable {
            private readonly SemaphoreSlim sem;

            public SemGuard(SemaphoreSlim sem) {
                this.sem = sem;
            }

            public void Dispose() {
                sem.Release();
            }
        }
    }
}