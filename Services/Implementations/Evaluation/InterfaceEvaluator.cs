using IotDash.Data.Model;
using IotDash.Domain.Events;
using IotDash.Domain.Mediator;
using IotDash.Parsing;
using IotDash.Parsing.Expressions;
using IotDash.Services.Domain;
using IotDash.Services.Messaging;
using IotDash.Services.Mqtt;
using IotDash.Utils.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace IotDash.Services.Evaluation {
    internal sealed class InterfaceEvaluator : IDisposable, IMqttSubscriber, IInterfaceEvaluationContext {

        private readonly SubscriptionGuard guard = new();
        private readonly IotInterface entity;
        private readonly IServiceScope scope;
        private readonly ILogger<InterfaceEvaluator> logger;
        private readonly MqttMediator mqtt;
        private readonly IExpr expressionTree;

        public double? Value { get; private set; } = null;

        public InterfaceEvaluator(IotInterface iface, IServiceScope scope) {
            var provider = scope.ServiceProvider;
            this.entity = iface;
            this.scope = scope;
            this.logger = provider.GetRequiredService<ILogger<InterfaceEvaluator>>();
            this.mqtt = provider.GetRequiredService<MqttMediator>();


            if (iface.Expression == null) {
                throw new ArgumentException("Cannot evaluate interface withou an expression.");
            }

            this.expressionTree = ExpressionsParser.ParseOrThrow(iface.Expression);

            ISet<string> referencedTopics = new HashSet<string>();
            expressionTree.Traverse(expr => _ = expr is TopicRef topicRef && referencedTopics.Add(topicRef.Topic));

            foreach (var t in referencedTopics) {
                mqtt.Subscribe(t, this, guard);
            }
        }

        async Task ITarget<string, MqttApplicationMessage>.OnReceive(object? sender, MqttApplicationMessage message) {
            await this.Update();
        }

        /// <summary>
        /// Evaluate and publish change
        /// </summary>
        /// <returns>Task that completes when published.</returns>
        public async Task Update() {
            bool changed = Evaluate();
            if (changed) {
                Debug.Assert(Value.HasValue);
                entity.Value = (double)Value;
                await Publish();
            }
        }

        DateTime IInterfaceEvaluationContext.GetNow() => DateTime.Now;
        double IInterfaceEvaluationContext.GetValue(string topic) {
            var lastMsg = mqtt.GetRetained(topic);
            double value = 0;
            if (lastMsg != null) {
                string payload = lastMsg.ConvertPayloadToString();
                double.TryParse(payload, out value);
            }

            return value;
        }

        /// <summary>
        /// Evaluate the interface and store the result in <see cref="Value"/>.
        /// </summary>
        /// <returns>True if value has changed.</returns>
        internal bool Evaluate() => this.Evaluate(this);

        /// <summary>
        /// Evaluate the interface and store the result in <see cref="Value"/>.
        /// </summary>
        /// <returns>True if value has changed.</returns>
        internal bool Evaluate(IInterfaceEvaluationContext context) {
            double newValue = double.NaN;
            try {
                newValue = expressionTree.Evaluate(context);
            } finally {
                logger.LogTrace($"Evaluating expression tree {expressionTree}\n   => {newValue}");
            }

            if (Value != newValue && !double.IsNaN(newValue)) {
                Value = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Publish the <see cref="Value"/> over MQTT.
        /// </summary>
        /// <returns></returns>
        private Task Publish() {
            if (Value == null) throw new InvalidOperationException("There is no value to publish.");
            Debug.Assert(Value.HasValue);
            return mqtt.Send(this.entity.GetTopicName(), this, Value.Value.ToString());
        }

        public void Dispose() {
            guard.Dispose();
        }
    }
}