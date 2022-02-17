using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace IotDash.Utils.Threading {

    /// <summary>
    /// Useful for Fire-and-forget tasks. Collects them in a queue and unwraps the exceptions.
    /// </summary>
    internal sealed class TaskUnwrapper {
        private readonly ILogger logger;
        private CancellationToken token;
        private readonly BufferBlock<Task> queue;

        public bool Running { get; private set; }

        public TaskUnwrapper(IServiceProvider provider) {
            this.logger = provider.GetRequiredService<ILogger<TaskUnwrapper>>();
            this.queue = new(new DataflowBlockOptions {
                CancellationToken = token,
                EnsureOrdered = false,
            });

            Running = false;
        }

        public void Unwrap(Task t) {
            if (!queue.Post(t)) {
                throw new Exception("Task was not accepted.");  
            }
        }

        public void Start(CancellationToken token) {
            if (!Running) {
                this.token = token;
                runningTask = runInternal();
            } else {
                throw new InvalidOperationException("Cannot run twice.");
            }
        }

        public Task Stop() {
            keepRunning = false;
            Unwrap(Task.CompletedTask);
            //await runningTask;
            return Task.CompletedTask;
        }

        private bool keepRunning = true;
        private Task<Task> dequeueTask;
        private Task runningTask;
        private async Task runInternal() {
            Running = true;
            HashSet<Task> tasks = new();
            try {
                while (!token.IsCancellationRequested && keepRunning) {
                    try {

                        dequeueTask = queue.ReceiveAsync(token);
                        var any = await Task.WhenAny(tasks.Append(dequeueTask));

                        if (any == dequeueTask) {
                            var newTask = await dequeueTask;
                            tasks.Add(newTask);
                        } else {
                            // unwrap the task
                            await any;
                            tasks.Remove(any);
                        }

                    } catch (Exception e) {
                        logger.LogCritical(e, $"Unhandled exception.");
                    }
                }
            } finally {
                Running = false;
            }
        }
    }

}