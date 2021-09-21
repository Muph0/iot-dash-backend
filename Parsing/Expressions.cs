using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IotDash.Parsing.Expressions;
using TValue = System.Double;

namespace IotDash.Parsing {

    

}


namespace IotDash.Parsing.Expressions {

    interface IExpr : IEquatable<IExpr> {
        void Traverse(IVisitor visitor);
        void Traverse(Action<IExpr> visitor)
            => Traverse(new PreorderVisitor(visitor));

        TValue Evaluate(InterfaceEvaluationContext context) {
            var visitor = new EvaluatingVisitor(context);
            Traverse(visitor);
            return visitor.Result;
        }        
    }

    class Literal : IExpr {
        public TValue Value { get; }
        public Literal(TValue value) {
            Value = value;
        }

        public bool Equals(IExpr? other)
            => other is Literal l && this.Value == l.Value;

        public void Traverse(IVisitor visitor) {
            visitor.Visit(this, 0);
        }
    }

    class UnaryOp : IExpr {
        public enum Types { Neg }
        public Types Type { get; }
        public IExpr Expr { get; }

        public UnaryOp(Types type, IExpr expr) {
            Type = type;
            Expr = expr;
        }

        public bool Equals(IExpr? other)
            => other is UnaryOp u
            && Type == u.Type
            && Expr.Equals(u.Expr);

        public void Traverse(IVisitor visitor) {
            visitor.Visit(this, 0);
            Expr.Traverse(visitor);
            visitor.Visit(this, 1);
        }
    }

    class BinaryOp : IExpr {
        public enum Types { Add, Sub, Mul, Div, Less, Greater, LessEq, GreaterEq, LAnd, LOr,
            Equal,
            Mod
        }

        public Types Type { get; }
        public IExpr Left { get; }
        public IExpr Right { get; }

        public BinaryOp(Types type, IExpr left, IExpr right) {
            Type = type;
            Left = left;
            Right = right;
        }

        public bool Equals(IExpr? other)
            => other is BinaryOp b
            && this.Type == b.Type
            && Left.Equals(b.Left)
            && Right.Equals(b.Right);

        public void Traverse(IVisitor visitor) {
            visitor.Visit(this, 0);
            Left.Traverse(visitor);
            visitor.Visit(this, 1);
            Right.Traverse(visitor);
            visitor.Visit(this, 2);
        }
    }

    class FunctionCall : IExpr {
        public string FunctionName { get; }
        public ImmutableArray<IExpr> Arguments { get; }

        public FunctionCall(string function, IEnumerable<IExpr> arguments) {
            FunctionName = function;
            Arguments = arguments.ToImmutableArray();
        }

        public bool Equals(IExpr? other)
            => other is FunctionCall f
            && FunctionName == f.FunctionName
            && Arguments.SequenceEqual(f.Arguments);

        public void Traverse(IVisitor visitor) {
            int i = 0;
            visitor.Visit(this, i++);
            foreach (var arg in Arguments) {
                arg.Traverse(visitor);
                visitor.Visit(this, i++);
            }
            visitor.Visit(this, i++);
        }
    }

    class TopicRef : IExpr {
        public string Topic { get; }

        public TopicRef(string topic) {
            Topic = topic;
        }

        public bool Equals(IExpr? other)
            => other is TopicRef t
            && Topic == t.Topic;

        public void Traverse(IVisitor visitor) {
            visitor.Visit(this, 0);
        }
    }
}
