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
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace IotDash.Services.Evaluation {
    internal sealed class InterfaceEvaluator : IDisposable, IMqttSubscriber, IInterfaceEvaluationContext {

        private readonly SubscriptionGuard guard = new();
        private readonly IotInterface entity;
        private readonly IServiceScope scope;
        private readonly ILogger<InterfaceEvaluator> logger;
        private readonly AMqttMediator mqtt;
        private readonly IExpr expressionTree;

        public double? Value { get; private set; }

        public InterfaceEvaluator(IotInterface iface, IServiceScope scope) {
            var provider = scope.ServiceProvider;
            this.entity = iface;
            this.scope = scope;
            this.logger = provider.GetRequiredService<ILogger<InterfaceEvaluator>>();
            this.mqtt = provider.GetRequiredService<AMqttMediator>();


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
            bool changed = Evaluate();
            if (changed) {
                await Publish();
            }
        }


        double IInterfaceEvaluationContext.GetValue(string topic) {
            var lastMsg = mqtt.GetRetained(topic);
            double value = 0;
            if (lastMsg != null)
                double.TryParse(lastMsg.ConvertPayloadToString(), out value);

            return value;
        }

        /// <summary>
        /// Evaluate the interface and store the result in <see cref="Value"/>.
        /// </summary>
        /// <returns>True if value has changed.</returns>
        public bool Evaluate() {
            var newValue = expressionTree.Evaluate(this);

            if (Value != newValue) {
                Value = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Publish the <see cref="Value"/> over MQTT.
        /// </summary>
        /// <returns></returns>
        public Task Publish() {
            if (Value == null) throw new InvalidOperationException("There is no value to publish.");
            return mqtt.Send(this.entity.GetTopicName(), this, this.Value.ToString());
        }

        public void Dispose() {
            guard.Dispose();
        }
    }
}