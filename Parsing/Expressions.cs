using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IotDash.Parsing.Expressions;
using TValue = System.Double;


namespace IotDash.Parsing.Expressions {

    /// <summary>
    /// A contract for all expression tree nodes.
    /// </summary>
    interface IExpr : IEquatable<IExpr> {

        /// <summary>
        /// Accept a <paramref name="visitor"/> to this node.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        void Traverse(IVisitor visitor);

        /// <summary>
        /// Traverse in preorder from this node down.
        /// </summary>
        /// <param name="visitor">An action that is called with all encountered nodes in preorder.</param>
        void Traverse(Action<IExpr> visitor)
            => Traverse(new PreorderVisitor(visitor));

        /// <summary>
        /// Traverse the tree with an <see cref="EvaluatingVisitor"/>.
        /// </summary>
        /// <param name="context">Context for this evaluation.</param>
        /// <returns>Value of the expression.</returns>
        TValue Evaluate(IInterfaceEvaluationContext context) {
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

        public override string ToString() {
            return Value.ToString();
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

        public override string ToString() {
            return $"'{Type}' {Expr}";
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

        public override string ToString() {
            return $"({Left} '{Type}' {Right})";
        }
    }

    class FunctionCall : IExpr {
        public string FunctionName { get; }
        public ImmutableArray<IExpr> Arguments { get; }

        public FunctionCall(string function, IEnumerable<IExpr> arguments) {
            if (!FunctionDefinition.All.ContainsKey((function, arguments.Count()))) {
                throw new FunctionNotDefinedException(function, arguments.Count());
            }

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

        public override string ToString() {
            return $"{FunctionName}({string.Join(",", Arguments.Select(arg => arg.ToString()))})";
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

        public override string ToString() {
            return $"[\"{Topic}\"]";
        }
    }
}
