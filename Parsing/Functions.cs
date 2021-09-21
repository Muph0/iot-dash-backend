using IotDash.Domain;
using IotDash.Installers;
using IotDash.Parsing;
using IotDash.Parsing.Expressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using TValue = System.Double;

namespace IotDash.Parsing {

    class FunctionDefinition {
        public string Name { get; }
        public int ArgCount { get; }

        public delegate TValue EvalDelegate(ReadOnlySpan<TValue> args);
        private EvalDelegate eval;

        public static TValue Evaluate(string functionName, ReadOnlySpan<TValue> args) {
            var func = All[(functionName, args.Length)];
            return func.eval(args);
        }

        public FunctionDefinition(string name, int argCount, EvalDelegate eval) {
            Name = name;
            ArgCount = argCount;
            this.eval = eval;
        }

        private static void Define(FunctionDefinition function) {
            all.Add((function.Name, function.ArgCount), function);
        }

        private static readonly Dictionary<(string, int), FunctionDefinition> all = new();
        public static IReadOnlyDictionary<(string, int), FunctionDefinition> All => all;

        public static void Initialize() {
            if (all.Count > 0) {
                return;
            }

            // analytic
            Define(new("exp", 1, args => Math.Exp(args[0])));
            Define(new("log", 1, args => Math.Log(args[0])));
            Define(new("log", 2, args => Math.Log(args[0], args[1])));
            Define(new("pow", 2, args => Math.Pow(args[0], args[1])));

            // date-time
            Define(new("time_of_day", 0, args => DateTime.Now.TimeOfDay.TotalSeconds));
            Define(new("day_of_week", 0, args => (int)DateTime.Now.DayOfWeek));
            Define(new("day_of_year", 0, args => DateTime.Now.DayOfYear));
            Define(new("month", 0, args => DateTime.Now.Month));

            // steps
            Define(new("floor", 1, args => Math.Floor(args[0])));
            Define(new("ceil", 1, args => Math.Ceiling(args[0])));
            Define(new("abs", 1, args => Math.Abs(args[0])));
            Define(new("if", 3, args => EvaluatingVisitor.dbool(args[0]) ? args[1] : args[2]));
        }
    }

    public class FunctionInstaller : IInstaller {
        public void InstallServices(IServiceCollection services, IConfiguration configuration) {
            FunctionDefinition.Initialize();
        }
    }
}
