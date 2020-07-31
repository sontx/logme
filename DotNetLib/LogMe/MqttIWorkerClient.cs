using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogMe
{
    internal class MqttIWorkerClient : IWorkerClient, IConnectingFailedHandler, IMqttClientConnectedHandler, IMqttClientDisconnectedHandler
    {
        private readonly string clientName;
        private readonly string url;
        private readonly string appName;
        private readonly ManualResetEvent startWaitEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent stopWaitEvent = new ManualResetEvent(false);
        private string connectingFailedReason;
        private IManagedMqttClient mqttClient;

        public MqttIWorkerClient(string clientName, string url, string appName)
        {
            this.clientName = clientName;
            this.url = url;
            this.appName = appName;
        }

        public void Send(string message, MessageType messageType)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync()
        {
            connectingFailedReason = null;

            var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId(clientName)
                .WithWebSocketServer(url)
                .WithCleanSession()
                .Build())
            .Build();

            return Task.Run(async () =>
            {
                if (mqttClient != null)
                    await mqttClient.StopAsync();
                mqttClient = new MqttFactory().CreateManagedMqttClient();

                mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    var msg = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var topic = e.ApplicationMessage.Topic;
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

        public Task StopAsync()
        {
            throw new NotImplementedException();
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