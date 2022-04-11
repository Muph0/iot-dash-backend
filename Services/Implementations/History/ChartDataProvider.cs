using IotDash.Domain.Mediator;
using IotDash.Services.Mqtt;
using MQTTnet;
using System;
using System.Threading.Tasks;

namespace IotDash.Services.History {
    internal class ChartDataProvider : IMessageTarget<MqttApplicationMessage> {
        
        
        public Task OnReceive(object? sender, MqttApplicationMessage message) {
            throw new NotImplementedException();
        }
    }
}
