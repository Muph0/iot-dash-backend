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

    class InterfaceEvaluationContext {
        public InterfaceEvaluationContext(Dictionary<string, TopicValueSubscription>? topics) {
            Debug.Assert(topics != null);
            Topics = topics;
        }

        public IReadOnlyDictionary<string, TopicValueSubscription> Topics { get; init; }
        public double DefaultValue => 0.0;
    }

    class EvaluatingVisitor : IRecursiveVisitor<TValue> {

        List<TValue> IRecursiveVisitor<TValue>.stack => this.stack;
        private readonly List<TValue> stack = new();
        private readonly InterfaceEvaluationContext context;
        public TValue Result => ((IRecursiveVisitor<TValue>)this).GetResult();

        public EvaluatingVisitor(InterfaceEvaluationContext context) {
            this.context = context;
        }

        TValue IRecursiveVisitor<TValue>.Visit(Literal literal) {
            return literal.Value;
        }
        TValue IRecursiveVisitor<TValue>.Visit(TopicRef topicRef) {
            return context.Topics[topicRef.Topic].Value ?? context.DefaultValue;
        }

        TValue IRecursiveVisitor<TValue>.Visit(UnaryOp op, TValue arg) {
            switch (op.Type) {
                case UnaryOp.Types.Neg: return -arg;
                default: throw new NotImplementedException();
            }
        }

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
