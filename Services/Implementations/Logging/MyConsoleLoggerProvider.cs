using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace IotDash.Services {
    internal class MyConsoleLoggerProvider : ILoggerProvider {

        private object consoleLock = new object();
        private readonly IDictionary<string, int> categoryCount = new Dictionary<string, int>();


        public ILogger CreateLogger(string categoryName) {
            if (categoryCount.ContainsKey(categoryName)) {
                categoryCount[categoryName]++;
            } else {
                categoryCount[categoryName] = 0;
            }

            return new MyConsoleLogger(this, $"{categoryName}[{categoryCount[categoryName]}]");
        }

        public void Dispose() {
            // do nothing
        }

        internal class MyConsoleLogger : ILogger {

            class LoggerScope<TState> : IDisposable {
                private readonly MyConsoleLogger logger;
                private readonly TState state;

                public LoggerScope(MyConsoleLogger logger, TState state) {
                    this.logger = logger;
                    this.state = state;
                    logger.scopes.Push(this);
                }

                public void Dispose() {
                    var scope = logger.scopes.Pop();
                }
            }

            private readonly MyConsoleLoggerProvider provider;
            private readonly string categoryName;
            private readonly Stack<IDisposable> scopes = new();

            public MyConsoleLogger(MyConsoleLoggerProvider provider, string categoryName) {
                this.provider = provider;
                this.categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) {
                return new LoggerScope<TState>(this, state);
            }

            public bool IsEnabled(LogLevel logLevel) {
                return true;
            }

            private static Dictionary<LogLevel, (string, ConsoleColor)> logLevelTags = new() {
                { LogLevel.Trace, ("Trace", ConsoleColor.DarkCyan) },
                { LogLevel.Debug, ("Debug", ConsoleColor.DarkMagenta) },
                { LogLevel.Information, ("Info", ConsoleColor.DarkGreen) },
                { LogLevel.Warning, ("Warn", ConsoleColor.Yellow) },
                { LogLevel.Error, ("Error", ConsoleColor.DarkRed) },
                { LogLevel.Critical, ("Critical", ConsoleColor.Red) },
            };

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
                var (level, color) = logLevelTags[logLevel];
                level = (level + ':').PadRight(5) + ' ';
                var pad = new string(' ', 4);

                lock (provider.consoleLock) {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(categoryName + ":");
                    Console.Write($"[{DateTime.Now.ToLongTimeString()}] ");
                    Console.ForegroundColor = color;
                    Console.Write(level);
                    Console.ResetColor();
                    Console.WriteLine(formatter(state, exception));

                    if (exception != null) {
                        Console.WriteLine(pad + "Reason: " + exception.Message);
#if DEBUG
                        bool debug = true;
#else
                    bool debug = false;
#endif
                        if (debug && logLevel == LogLevel.Critical) {
                            Console.WriteLine(exception.StackTrace);
                        }
                    }
                }
            }
        }
    }
}