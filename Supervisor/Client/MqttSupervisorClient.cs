using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class MqttSupervisorClient : ISupervisorClient, IConnectingFailedHandler, IMqttClientConnectedHandler, IMqttClientDisconnectedHandler
    {
        public Action<string> OnMessage { get; set; }

        private readonly string name;
        private readonly string url;
        private readonly string topic;
        private readonly ManualResetEvent startWaitEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent stopWaitEvent = new ManualResetEvent(false);
        private string connectingFailedReason;
        private IManagedMqttClient mqttClient;

        public MqttSupervisorClient(string name, string url, string topic)
        {
            this.name = name;
            this.url = url;
            this.topic = topic;
        }

        public Task StartAsync()
        {
            connectingFailedReason = null;

            var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId(name)
                .WithWebSocketServer(url)
                .WithCleanSession()
                .Build())
            .Build();

            return Task.Run(async () =>
            {
                if (mqttClient != null)
                    await mqttClient.StopAsync();
                mqttClient = new MqttFactory().CreateManagedMqttClient();
                await mqttClient.SubscribeAsync(new MqttTopicFilter { Topic = topic, QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce });
                mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    OnMessage?.Invoke(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                });
                mqttClient.ConnectedHandler = this;
                mqttClient.ConnectingFailedHandler = this;
                mqttClient.DisconnectedHandler = this;
                startWaitEvent.Reset();
                await mqttClient.StartAsync(options);
                startWaitEvent.WaitOne();
                if (!string.IsNullOrEmpty(connectingFailedReason))
                    throw new Exception(connectingFailedReason);
            });
        }

        public async Task StopAsync()
        {
            startWaitEvent?.Dispose();
            if (mqttClient != null)
            {
                await mqttClient.StopAsync();
                stopWaitEvent.WaitOne();
                stopWaitEvent?.Dispose();
                mqttClient = null;
            }
        }

        public Task HandleConnectingFailedAsync(ManagedProcessFailedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                connectingFailedReason = eventArgs.Exception.Message;
                startWaitEvent.Set();
            });
        }

        public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                connectingFailedReason = null;
                startWaitEvent.Set();
            });
        }

        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                stopWaitEvent.Set();
            });
        }
    }
}