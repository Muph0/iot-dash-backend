using MQTTnet;

namespace IotDash.Utils.Debugging {
    internal static class DebuggingExtension {
        public static string ToDebugString(this MqttApplicationMessage msg) {
            return $"{{[Q{(int)msg.QualityOfServiceLevel} \"{msg.Topic}\"]: \"{msg.ConvertPayloadToString()}\"}}";
        }
    }
}
