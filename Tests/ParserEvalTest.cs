using NUnit.Framework;
using IotDash.Parsing;
using Tests.Adapters;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IotDash.Migrations;

namespace IotDash.Tests {
    public class ParserEvalTest {

        public const double TOLERATED_ERROR = 0.0001;
        TestEvaluationContext context;

        private void AssertParseEval(double expectedOutput, string input, double maxError = TOLERATED_ERROR) {
            var expr = ExpressionsParser.ParseOrThrow(input);

            double actualOutput = expr.Evaluate(context);
            double error = Math.Abs(actualOutput - expectedOutput);
            Assert.IsTrue(error <= maxError, $"Expected '{input}' to be within {maxError} of {expectedOutput}. Was {actualOutput}");
        }

        [SetUp]
        public void SetUp() {
            context = new TestEvaluationContext();
            context.FrozenTime = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Local);
        }

        [TestCase("123.4", 123.4)]
        [TestCase("0.1 + 0.2", 0.3)]
        [TestCase("11 - 0.2", 10.8)]
        [TestCase("1.6 * 8", 12.8)]
        [TestCase("8 * -3", -24.0)]
        [TestCase("100 / 3", 33.333333)]
        [TestCase("123456 mod 100", 56.0)]
        [TestCase("123456 mod 1000", 456.0)]
        [TestCase("1 = 1", 1.0)]
        [TestCase("1 = 2", 0.0)]
        [TestCase("1 > 1", 0.0)]
        [TestCase("1 > 2", 0.0)]
        [TestCase("2 > 1", 1.0)]
        [TestCase("1 >= 1", 1.0)]
        [TestCase("1 >= 2", 0.0)]
        [TestCase("2 >= 1", 1.0)]
        [TestCase("1 < 1", 0.0)]
        [TestCase("1 < 2", 1.0)]
        [TestCase("2 < 1", 0.0)]
        [TestCase("1 <= 1", 1.0)]
        [TestCase("1 <= 2", 1.0)]
        [TestCase("2 <= 1", 0.0)]
        [TestCase("1 and 1", 1.0)]
        [TestCase("0 and 1", 0.0)]
        [TestCase("1 and 0", 0.0)]
        [TestCase("0 and 0", 0.0)]
        [TestCase("1 or 1", 1.0)]
        [TestCase("0 or 1", 1.0)]
        [TestCase("1 or 0", 1.0)]
        [TestCase("0 or 0", 0.0)]
        [TestCase("if(1, 10, 20)", 10.0)]
        [TestCase("if(0, 10, 20)", 20.0)]
        [TestCase("floor(7.999)", 7.0)]
        [TestCase("floor(-7.999)", -8.0)]
        [TestCase("floor(7.0)", 7.0)]
        [TestCase("floor(-7.0)", -7.0)]
        [TestCase("ceil(7.999)", 8.0)]
        [TestCase("ceil(-7.999)", -7.0)]
        [TestCase("ceil(7.0)", 7.0)]
        [TestCase("ceil(-7.0)", -7.0)]
        [TestCase("abs(12.345)", 12.345)]
        [TestCase("abs(-7.654)", 7.654)]
        [TestCase("time_of_day()", 3.0 * 3600 + 4 * 60 + 5)]
        [TestCase("day_of_week()", 4)]
        [TestCase("day_of_year()", 2)]
        [TestCase("month()", 1)]
        [TestCase("pow(pow(2,0.5), 2)", 2)]
        public void ExpressionEvals(string input, double result, double maxError = TOLERATED_ERROR) {
            AssertParseEval(result, input);
        }


        [TestCase(5.0, 1.0, "x + y", 6.0)]
        [TestCase(null, 1.0, "x + y", 1.0)]
        [TestCase(5.0, null, "x + y", 5.0)]
        [TestCase(5.0, 1.0, "x + y + 1", 7.0)]
        public void ExpressionWithTopicsXYEvals(double? x, double? y, string input, double result, double maxError = TOLERATED_ERROR) {
            input = Regex.Replace(input, "(x|y)", "[\"$1\"]");

            if (x.HasValue) context.SetValue("x", x.Value);
            if (y.HasValue) context.SetValue("y", y.Value);

            AssertParseEval(result, input);
        }

        [Test]
        public void DaysOfWeekAreInCorrectOrder() {
            var days = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

            var now = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Local);

            foreach (var day in days) {
                while (now.DayOfWeek != day) {
                    now = now.AddDays(1);
                }

                int dayNumber = Array.IndexOf(days, day) + 1;
                context.FrozenTime = now;
                AssertParseEval(dayNumber, "day_of_week()");
            }
        }
    }
}