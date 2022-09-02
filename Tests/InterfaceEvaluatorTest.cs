using IotDash.Data.Model;
using IotDash.Services.Evaluation;
using IotDash.Services.Mqtt;
using IotDash.Tests.Adapters;
using Microsoft.Extensions.Logging;
using MQTTnet;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Tests.Adapters;

namespace IotDash.Tests {
    public class InterfaceEvaluatorTest {

        Queue<(object sender, MqttApplicationMessage msg)> sentMessages;
        TestMqttMediator mediator;
        TestCaseScope scope;

        IotInterface iface;
        InterfaceEvaluator evaluator;

        public static void AssertAreClose(double expected, double actual, double maxError = ParserEvalTest.TOLERATED_ERROR) {
            var error = Math.Abs(expected - actual);
            Assert.IsTrue(error <= maxError, $"Expected '{actual}' to be within {maxError} of {expected}.");
        }
        public static void AssertAreClose(double expected, MqttApplicationMessage actual, double maxError = ParserEvalTest.TOLERATED_ERROR) {
            double actualValue = double.Parse(actual.ConvertPayloadToString());
            var error = Math.Abs(expected - actualValue);
            Assert.IsTrue(error <= maxError, $"Expected '{actualValue}' to be within {maxError} of {expected}.");
        }

        [SetUp]
        public void SetUp() {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            sentMessages = new();
            mediator = new(sentMessages);
            scope = new();

            iface = new() {
                Id = Guid.NewGuid(),
                Kind = IotDash.Contracts.V1.Model.InterfaceKind.Switch,
                HistoryEnabled = false,
                Topic = "out",
                Expression = "[\"in\"] + 1",
            };

            scope.AddMock<ILogger<InterfaceEvaluator>>();
            scope.AddService(typeof(MqttMediator), mediator);

            evaluator = new(iface, scope);
        }

        [Test]
        public void EvaluatorEvaluatesCorrectValue() {

            var context = new TestEvaluationContext();
            context.SetValue("in", 50.0);

            evaluator.Evaluate(context);

#nullable disable
            AssertAreClose(51, (double)evaluator.Value);
#nullable restore
        }

        [TestCase(1.0)]
        [TestCase(-8.0)]
        public async Task EvaluatorRespondsToMqttMessage(double testValue) {
            await mediator.Send("in", this, new(testValue.ToString()));
            sentMessages.Dequeue();

            Assert.IsTrue(sentMessages.Count == 1);
            var response = sentMessages.Dequeue();

            Assert.AreEqual(evaluator, response.sender);
            Assert.AreEqual("out", response.msg.Topic);
            AssertAreClose(testValue + 1, response.msg);
        }

        [TestCase(12.0)]
        [TestCase(-3.1415926)]
        public async Task EvaluatorChangesInterfaceValue(double testValue) {
            await mediator.Send("in", this, new(testValue.ToString()));
            sentMessages.Dequeue();

            AssertAreClose(testValue + 1, iface.Value);
        }
    }
}
