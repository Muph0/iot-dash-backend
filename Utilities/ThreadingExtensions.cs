using System;
using System.Threading;
using System.Threading.Tasks;

namespace IotDash.Utils.Threading {

    /// <summary>
    /// Extensions for <see cref="SemaphoreSlim"/> synchronisation primitive allowing nice syntax with the <c>using (...)</c> pattern.
    /// </summary>
    internal static class ThreadingExtensions {

        /// <summary>
        /// Internally does <see cref="SemaphoreSlim.WaitAsync()"/> and returns a semaphore guard.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <returns>An <see cref="IDisposable"/> object, that when disposed releases the semaphore.</returns>
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