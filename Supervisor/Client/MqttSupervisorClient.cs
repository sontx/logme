using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class MqttSupervisorClient : ISupervisorClient
    {
        public Action<string> OnMessage { get; set; }

        private readonly string name;
        private readonly string url;
        private readonly string topic;
        private IManagedMqttClient mqttClient;

        public MqttSupervisorClient(string name, string url, string topic)
        {
            this.name = name;
            this.url = url;
            this.topic = topic;
        }

        public async Task StartAsync()
        {
            var options = new ManagedMqttClientOptionsBuilder()
               .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
               .WithClientOptions(new MqttClientOptionsBuilder()
                   .WithClientId(name)
                   .WithWebSocketServer(url)
                   .WithCleanSession()
                   .Build())
               .Build();

            if (mqttClient != null)
                await mqttClient.StopAsync();
            mqttClient = new MqttFactory().CreateManagedMqttClient();
            await mqttClient.SubscribeAsync(new MqttTopicFilter { Topic = topic, QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce });
            await mqttClient.StartAsync(options);
            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                OnMessage?.Invoke(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
            });
        }

        public async Task StopAsync()
        {
            if (mqttClient != null)
            {
                await mqttClient.StopAsync();
                mqttClient = null;
            }
        }
    }
}
