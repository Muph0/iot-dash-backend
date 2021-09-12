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
        public InterfaceEvaluationContext(Dictionary<string, TopicHandler>? topics) {
            Debug.Assert(topics != null);
            Topics = topics;
        }

        public IReadOnlyDictionary<string, TopicHandler> Topics { get; init; }
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

        TValue IRecursiveVisitor<TValue>.Visit(BinaryOp op, TValue left, TValue right) {
            switch (op.Type) {
                case BinaryOp.Types.Add: return left + right;
                case BinaryOp.Types.Sub: return left - right;
                case BinaryOp.Types.Mul: return left * right;
                case BinaryOp.Types.Div: return  left / right;
                default: throw new NotImplementedException();
            }
        }

        TValue IRecursiveVisitor<TValue>.Visit(FunctionCall functionCall, ReadOnlySpan<TValue> args) {
            return FunctionDefinition.Evaluate(functionCall.FunctionName, args);
        }
    }
}
