using IotDash.Domain;
using IotDash.Parsing;
using IotDash.Parsing.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using TValue = System.Double;

namespace IotDash.Parsing {

    interface IInterfaceEvaluationContext {
        TValue GetValue(string topic);
    }

    class InterfaceEvaluationContext : IInterfaceEvaluationContext {
        public InterfaceEvaluationContext(Dictionary<string, TopicValueSubscription> topics) {
            this.topics = topics;
        }

        IReadOnlyDictionary<string, TopicValueSubscription> topics;
        double defaultValue => 0.0;

        double IInterfaceEvaluationContext.GetValue(string topic) {
            return topics.ContainsKey(topic) ? topics[topic].Value ?? defaultValue : defaultValue;
        }
    }

    class EvaluatingVisitor : IRecursiveVisitor<TValue> {

        List<TValue> IRecursiveVisitor<TValue>.stack => this.stack;
        private readonly List<TValue> stack = new();
        private readonly IInterfaceEvaluationContext context;
        public TValue Result => ((IRecursiveVisitor<TValue>)this).GetResult();

        public EvaluatingVisitor(IInterfaceEvaluationContext context) {
            this.context = context;
        }

        TValue IRecursiveVisitor<TValue>.Visit(Literal literal) {
            return literal.Value;
        }
        TValue IRecursiveVisitor<TValue>.Visit(TopicRef topicRef) {
            return context.GetValue(topicRef.Topic);
        }

        TValue IRecursiveVisitor<TValue>.Visit(UnaryOp op, TValue arg)
            => op.Type switch {
                UnaryOp.Types.Neg => -arg,
                _ => throw new NotImplementedException(),
            };

        internal static double dbool(bool x) {
            return x ? 1 : 0;
        }
        internal static bool dbool(double x) {
            return x != 0;
        }

        TValue IRecursiveVisitor<TValue>.Visit(BinaryOp op, TValue left, TValue right) {
            switch (op.Type) {
                case BinaryOp.Types.Add: return left + right;
                case BinaryOp.Types.Sub: return left - right;
                case BinaryOp.Types.Mul: return left * right;
                case BinaryOp.Types.Div: return left / right;
                case BinaryOp.Types.Mod: return left % right;
                case BinaryOp.Types.Equal: return dbool(left == right);
                case BinaryOp.Types.Less: return dbool(left < right);
                case BinaryOp.Types.Greater: return dbool(left > right);
                case BinaryOp.Types.LessEq: return dbool(left <= right);
                case BinaryOp.Types.GreaterEq: return dbool(left >= right);
                case BinaryOp.Types.LAnd: return dbool(dbool(left) && dbool(right));
                case BinaryOp.Types.LOr: return dbool(dbool(left) || dbool(right));
                default: throw new NotImplementedException($"Operator {op.Type} not implemented.");
            }
        }

        TValue IRecursiveVisitor<TValue>.Visit(FunctionCall functionCall, ReadOnlySpan<TValue> args) {
            return FunctionDefinition.Evaluate(functionCall.FunctionName, args);
        }
    }
}
