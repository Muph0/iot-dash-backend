using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                Running = true;
                runningTask = runInternal();
            } else {
                throw new InvalidOperationException("Cannot run twice.");
            }
        }

        public Task Stop() {
            keepRunning = false;
            return runningTask;
        }

        private bool keepRunning = true;
        private Task dequeueTask, runningTask;
        private async Task runInternal() {
            HashSet<Task> tasks = new();
            try {
                while (!token.IsCancellationRequested && keepRunning) {
                    try {

                        dequeueTask = queue.ReceiveAsync(token);
                        tasks.Add(dequeueTask);

                        var any = await Task.WhenAny(tasks);

                        if (any == dequeueTask) {
                            tasks.Remove(dequeueTask);
                            var newTask = await (Task<Task>)dequeueTask;
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