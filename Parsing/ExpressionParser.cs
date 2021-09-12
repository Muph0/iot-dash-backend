using System;
using System.Collections.Immutable;

using IotDash.Parsing.Expressions;

using Pidgin;
using Pidgin.Expression;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace IotDash.Parsing {
    static class ExpressionsParser {

        private static readonly Parser<char, double> Double
            = Num.Select(whole => (double)whole).Or(
                Num.Then(Char('.'), (l, r) => l)
                .Then(Digit.AtLeastOnceString(), (ints, digits) => ints + double.Parse("0." + digits)));

        private static Parser<char, T> Tok<T>(Parser<char, T> token)
            => Try(token).Before(SkipWhitespaces);
        private static Parser<char, string> Tok(string token)
            => Tok(String(token));

        private static Parser<char, T> Parenthesised<T>(Parser<char, T> parser)
            => parser.Between(Tok("("), Tok(")"));

        private static Parser<char, Func<IExpr, IExpr, IExpr>> Binary(Parser<char, BinaryOp.Types> op)
            => op.Select<Func<IExpr, IExpr, IExpr>>(type => (l, r) => new BinaryOp(type, l, r));
        private static Parser<char, Func<IExpr, IExpr>> Unary(Parser<char, UnaryOp.Types> op)
            => op.Select<Func<IExpr, IExpr>>(type => o => new UnaryOp(type, o));

        private static readonly Parser<char, Func<IExpr, IExpr, IExpr>> Add
            = Binary(Tok("+").ThenReturn(BinaryOp.Types.Add));
        private static readonly Parser<char, Func<IExpr, IExpr, IExpr>> Sub
            = Binary(Tok("-").ThenReturn(BinaryOp.Types.Sub));
        private static readonly Parser<char, Func<IExpr, IExpr, IExpr>> Mul
            = Binary(Tok("*").ThenReturn(BinaryOp.Types.Mul));
        private static readonly Parser<char, Func<IExpr, IExpr, IExpr>> Div
            = Binary(Tok("*").ThenReturn(BinaryOp.Types.Div));
        private static readonly Parser<char, Func<IExpr, IExpr>> Neg
            = Unary(Tok("-").ThenReturn(UnaryOp.Types.Neg));

        private static readonly Parser<char, IExpr> Literal
            = Tok(Num)
                .Select<IExpr>(value => new Literal(value))
                .Labelled("integer literal");

        private static readonly Parser<char, IExpr> TopicRef
            = Tok(AnyCharExcept('"').AtLeastOnceString().Between(String("[\""), String("\"]")))
            .Select<IExpr>(str => new TopicRef(str.Trim('"')))
            .Labelled("topic reference");

        private static Parser<char, IExpr> FunctionCall(Parser<char, IExpr> expr)
            => Tok(Letter.Then(LetterOrDigit.ManyString(), (l, r) => l + r))
            .Then(
                Parenthesised(expr.Separated(Tok(","))),
                (l, r) => (IExpr)new FunctionCall(l, r)
                )
                .Labelled("function call");

        private static readonly Parser<char, IExpr> Expr = ExpressionParser.Build<char, IExpr>(
            expr => (
                OneOf(
                    Literal,
                    TopicRef,
                    FunctionCall(expr),
                    Parenthesised(expr).Labelled("parenthesised expression")
                ),
                new[]
                {
                    Operator.Prefix(Neg),
                    Operator.InfixL(Mul),
                    Operator.InfixL(Add)
                }
            )
        ).Labelled("expression");

        public static IExpr ParseOrThrow(string input)
            => Expr.ParseOrThrow(input);

        public static Result<char, IExpr> Parse(string input)
            => Expr.Parse(input);



    }
}
