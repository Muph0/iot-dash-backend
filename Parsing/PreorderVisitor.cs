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

    class PreorderVisitor : IVisitor {

        private readonly Action<IExpr> visitor;
        public PreorderVisitor(Action<IExpr> visitor) {
            this.visitor = visitor;
        }
        void InvokeVisitor(IExpr expr, int visitNo) {
            if (visitNo == 0) visitor.Invoke(expr);
        }

        public void Visit(Literal literal, int visitNo)
            => InvokeVisitor(literal, visitNo);
        public void Visit(TopicRef topicRef, int visitNo)
            => InvokeVisitor(topicRef, visitNo);
        public void Visit(UnaryOp op, int visitNo)
            => InvokeVisitor(op, visitNo);
        public void Visit(BinaryOp op, int visitNo)
            => InvokeVisitor(op, visitNo);
        public void Visit(FunctionCall functionCall, int visitNo)
            => InvokeVisitor(functionCall, visitNo);
    }
}
