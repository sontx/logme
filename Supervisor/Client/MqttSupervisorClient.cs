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
        public Action<string> OnLog { get; set; }
        public Action<string> OnException { get; set; }
        public Action<string> OnControlResponse { get; set; }

        private readonly string name;
        private readonly string url;
        private readonly string logTopic;
        private readonly string exceptionTopic;
        private readonly string controlTopic;
        private readonly ManualResetEvent startWaitEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent stopWaitEvent = new ManualResetEvent(false);
        private string connectingFailedReason;
        private IManagedMqttClient mqttClient;

        public MqttSupervisorClient(string name, string url)
        {
            this.name = name;
            this.url = url;
            this.logTopic = $"{name}/logs";
            this.exceptionTopic = $"{name}/exceptions";
            this.controlTopic = $"{name}/controls";
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
                await mqttClient.SubscribeAsync(new MqttTopicFilter
                {
                    Topic = logTopic,
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce
                }, new MqttTopicFilter
                {
                    Topic = exceptionTopic,
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce
                },
                new MqttTopicFilter
                {
                    Topic = controlTopic,
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce
                });
                mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    var msg = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var topic = e.ApplicationMessage.Topic;
                    if (topic == logTopic)
                        OnLog?.Invoke(msg);
                    else if (topic == exceptionTopic)
                        OnException?.Invoke(msg);
                    else if (topic == controlTopic)
                        OnControlResponse?.Invoke(msg);
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