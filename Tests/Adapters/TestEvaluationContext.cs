using IotDash.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Adapters {
    internal class TestEvaluationContext : IInterfaceEvaluationContext {

        private Dictionary<string, double> topicValues = new();

        public DateTime FrozenTime { get; set; } = DateTime.Now;
        DateTime IInterfaceEvaluationContext.GetNow() => FrozenTime;

        public TestEvaluationContext() { }
        public TestEvaluationContext(IDictionary<string, double> topicValues) {
            this.topicValues = new(topicValues);
        }

        public void SetValue(string topic, double value) {
            topicValues[topic] = value;
        }

        public double GetValue(string topic) {
            topicValues.TryGetValue(topic, out double value);
            return value;
        }

    }
}
