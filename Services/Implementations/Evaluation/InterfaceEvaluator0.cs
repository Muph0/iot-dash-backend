using IotDash.Parsing;
using IotDash.Parsing.Expressions;
using IotDash.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using MQTTnet;
using IotDash.Domain;
using System.Diagnostics;
using IotDash.Data.Model;
using Microsoft.Extensions.Logging;
using IotDash.Services.Mqtt;

namespace IotDash.Services.Evaluation {

    internal interface IManagedEvaluator : IDisposable {
        Task Evaluate(IServiceProvider scope);
    }


    internal class InterfaceEvaluator0 : IManagedEvaluator {

        private readonly IExpr expressionTree;
        private readonly IHostedMqttClient mqttClient;
        private readonly Guid ifaceKey;
        private readonly Dictionary<string, TopicValueSubscription> handlers;
        private readonly IEnumerable<string> outputTopics;
        private readonly ILogger logger;
        private double Value;

        public Guid Key => ifaceKey;

        public InterfaceEvaluator0(IServiceProvider provider, IExpr expressionTree, IotInterface iface, IEnumerable<TopicValueSubscription> handlers) {
            this.expressionTree = expressionTree;

            this.outputTopics = (new string?[] { iface.GetTopicName() }).Where(s => s != null).Cast<string>();
            this.mqttClient = provider.GetRequiredService<IHostedMqttClient>();

            this.logger = provider.GetRequiredService<ILogger<InterfaceEvaluator0>>();

            this.ifaceKey = iface.Id;
            this.handlers = handlers.ToDictionary(h => {
                // exclude self-reference updates
                if (!outputTopics.Contains(h.Topic.Name)) {
                    h.ValueUpdated += OnSubValueUpdated;
                }
                return h.Topic.Name;
            });
        }

        public static async Task<InterfaceEvaluator0> Create(IServiceProvider provider, IotInterface iface) {
            if (iface.Expression == null) {
                throw new ArgumentException("Cannot manage interface with no expression.");
            }

            var mqttClient = provider.GetRequiredService<IHostedMqttClient>();
            var expressionTree = ExpressionsParser.ParseOrThrow(iface.Expression);

            HashSet<string> usedTopics = new();
            expressionTree.Traverse(expr => _ = expr is TopicRef topicRef && usedTopics.Add(topicRef.Topic));

            List<TopicValueSubscription> handlers = new();
            foreach (var name in usedTopics) {
                var topic = await mqttClient.GetTopic(name);
                handlers.Add(new TopicValueSubscription(provider, topic));
            }

            InterfaceEvaluator0 result = new(provider, expressionTree, iface, handlers);

            if (handlers.Count == 0) {
                await result.Evaluate(provider);
            }

            return result;
        }

        private async Task OnSubValueUpdated(TopicUpdateEventArgs args) {
            if (((TopicValueSubscription)args.Subscriber).IsValueChanged) {
                await Evaluate(args.Context);
            }
        }

        public async Task Evaluate(IServiceProvider provider) {
            var ifaceStore = provider.GetRequiredService<IInterfaceStore>();

            var badHandlers = handlers.Values.Where(h => h.Value == null && !outputTopics.Contains(h.Topic.Name));
            if (badHandlers.Any()) {
                var topicNames = string.Join(", ", badHandlers.Select(h => $"'{h.Topic.Name}'"));
                logger.LogDebug($"Suspending evaluation due to missing topic(s) {topicNames}.");
                return;
            }

            var iface = await ifaceStore.GetByKeyAsync(ifaceKey);
            Debug.Assert(iface != null);

            Debug.Assert(expressionTree != null);
            var result = expressionTree.Evaluate(new InterfaceEvaluationContext(handlers));

            if (Value != result) {
                Value = result;

                iface.Value = Value;
                await ifaceStore.SaveChangesAsync();

                foreach (var topic in outputTopics) {
                    await mqttClient.Publish(topic, Value.ToString());
                }
            }

        }

        public void Dispose() {
            if (handlers != null) {
                foreach (var sub in handlers.Values) {
                    sub.Dispose();
                }
            }
        }
    }
}