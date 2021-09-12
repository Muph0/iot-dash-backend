using IotDash.Parsing;
using IotDash.Parsing.Expressions;
using IotDash.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using MQTTnet;
using IotDash.Domain;
using System.Diagnostics;
using IotDash.Data.Model;

namespace IotDash.Domain {

    class TopicHandler : IDisposable, ISubscriber {

        public ISubscribedTopic Topic { get; }
        public double? Value { get; private set; }

        private readonly ILogger logger;

        public delegate Task AsyncValueChangeHandler(TopicUpdateEventArgs args);
        public event AsyncValueChangeHandler ValueChanged;

        public TopicHandler(IServiceProvider provider, ISubscribedTopic topic) {
            this.logger = provider.GetRequiredService<ILogger<TopicHandler>>();
            this.Topic = topic;
            topic.Register(this);
        }

        public async Task OnMessageReceived(TopicUpdateEventArgs eventArgs) {

            string payload = Encoding.ASCII.GetString(eventArgs.MqttArgs.ApplicationMessage.Payload);

            if (double.TryParse(payload, out double newValue)) {
                if (Value != newValue) {
                    Value = newValue;

                    var evt = ValueChanged;
                    if (evt != null) {
                        foreach (var handler in evt.GetInvocationList().Cast<AsyncValueChangeHandler>()) {
                            await handler.Invoke(eventArgs);
                        }
                    }
                }
            } else {
                logger.LogError($"Failed to parse topic value '{payload}' as a decimal number.");
            }
        }

        public void Dispose() {
            Topic?.Unregister(this);
        }

    }

    class ManagedInterface : IDisposable {

        private readonly IExpr expressionTree;
        private readonly IHostedMqttClient mqttClient;
        private readonly (Guid, int) ifaceKey;
        private readonly Dictionary<string, TopicHandler> handlers;

        private double Value;

        public ManagedInterface(IServiceProvider provider, IExpr expressionTree, IotInterface iface, IEnumerable<TopicHandler> handlers) {
            this.expressionTree = expressionTree;

            this.mqttClient = provider.GetRequiredService<IHostedMqttClient>();

            this.ifaceKey = (iface.DeviceId, iface.Id);
            this.handlers = handlers.ToDictionary(h => {
                h.ValueChanged += OnSubValueChanged;
                return h.Topic.Name;
            });
        }

        public static async Task<ManagedInterface> Create(IServiceProvider provider, IotInterface iface) {
            if (iface.Expression == null) {
                throw new ArgumentException("Cannot manage interface with no expression.");
            }

            var mqttClient = provider.GetRequiredService<IHostedMqttClient>();
            var expressionTree = ExpressionsParser.ParseOrThrow(iface.Expression);

            HashSet<string> usedTopics = new();
            expressionTree.Traverse(expr => _ = expr is TopicRef topicRef && usedTopics.Add(topicRef.Topic));

            var handlers = await Task.WhenAll(usedTopics.Select(async name => {
                var topic = await mqttClient.GetTopic(name);
                return new TopicHandler(provider, topic);
            }));

            ManagedInterface result = new(provider, expressionTree, iface, handlers);

            if (handlers.Length == 0) {
                await result.Evaluate(provider);
            }

            return result;
        }

        private async Task OnSubValueChanged(TopicUpdateEventArgs args) {
            await Evaluate(args.Context);
        }

        public async Task Evaluate(IServiceProvider provider) {
            var ifaceStore = provider.GetRequiredService<IInterfaceStore>();

            var iface = await ifaceStore.GetByKeyAsync(ifaceKey);
            Debug.Assert(iface != null);

            Debug.Assert(expressionTree != null);
            var result = expressionTree.Evaluate(new InterfaceEvaluationContext(handlers));

            if (Value != result) {
                Value = result;

                iface.Value = Value;
                await ifaceStore.SaveChangesAsync();

                await mqttClient.Publish($"dev/{iface.DeviceId}/{iface.Id}", Value.ToString());

                if (iface.Device.Alias != null && iface.Alias != null) {
                    await mqttClient.Publish($"{iface.Device.Alias}/{iface.Alias}", Value.ToString());
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
