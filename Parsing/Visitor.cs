using IotDash.Domain;
using IotDash.Parsing;
using IotDash.Parsing.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using TValue = System.Double;

namespace IotDash.Parsing {

    interface IVisitor {
        void Visit(Literal literal, int visitNo);
        void Visit(TopicRef topicRef, int visitNo);
        void Visit(UnaryOp op, int visitNo);
        void Visit(BinaryOp op, int visitNo);
        void Visit(FunctionCall functionCall, int visitNo);
    }

    interface IRecursiveVisitor<TResult> : IVisitor {
        protected List<TResult> stack { get; }
        TResult GetResult() => stack.Single();

        TResult Result { get; }

        TResult Visit(Literal literal);
        TResult Visit(TopicRef topicRef);
        TResult Visit(UnaryOp op, TResult arg);
        TResult Visit(BinaryOp op, TResult left, TResult right);
        TResult Visit(FunctionCall functionCall, ReadOnlySpan<TResult> args);

        void IVisitor.Visit(Literal literal, int visitNo)
            => stack.Add(Visit(literal));
        void IVisitor.Visit(TopicRef topicRef, int visitNo)
            => stack.Add(Visit(topicRef));
        void IVisitor.Visit(UnaryOp op, int visitNo) {
            if (visitNo == 1) {
                stack[^1] = Visit(op, stack[^1]);
            }
        }
        void IVisitor.Visit(BinaryOp op, int visitNo) {
            if (visitNo == 2) {
                stack[^2] = Visit(op, stack[^2], stack[^1]);
                stack.RemoveAt(stack.Count - 1);
            }
        }
        void IVisitor.Visit(FunctionCall functionCall, int visitNo) {
            int argCount = functionCall.Arguments.Length;

            if (visitNo == argCount) {
                ReadOnlySpan<TResult> args = CollectionsMarshal.AsSpan(stack)
                    .Slice(stack.Count - argCount);

                stack[^argCount] = Visit(functionCall, args);
                stack.RemoveRange(stack.Count - argCount + 1, argCount - 1);
            }
        }
    }
}
