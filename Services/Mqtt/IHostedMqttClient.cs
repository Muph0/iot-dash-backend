using IotDash.Domain;
using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Receiving;
using System.Threading.Tasks;

namespace IotDash.Services.Mqtt {

    interface IHostedMqttClient : IHostedService,
                                         IMqttClientConnectedHandler,
                                         IMqttClientDisconnectedHandler,
                                         IMqttApplicationMessageReceivedHandler {

        Task<ISubscribedTopic> GetTopic(string topic);
        Task Publish(string topicId, string value);
    }

}