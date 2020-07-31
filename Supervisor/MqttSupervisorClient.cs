using LogMe.Core;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading.Tasks;

namespace LogMe.Supervisor
{
    public class MqttSupervisorClient : AbstractMqttClient, ISupervisorClient
    {
        public Action<string> OnLog { get; set; }
        public Action<string> OnException { get; set; }
        public Action<string> OnControlResponse { get; set; }

        private readonly string logTopic;
        private readonly string exceptionTopic;
        private readonly string controlTopic;
        private readonly string controlResponseTopic;

        public MqttSupervisorClient(string name, string url)
            : base(name, url)
        {
            this.logTopic = $"{name}/logs";
            this.exceptionTopic = $"{name}/exceptions";
            this.controlTopic = $"{name}/controls";
            this.controlResponseTopic = $"{name}/controls/response";
        }

        protected async override void OnConnected()
        {
            base.OnConnected();

            await MqttClient.SubscribeAsync(new MqttTopicFilter
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
                Topic = controlResponseTopic,
                QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce
            });
        }

        protected override void OnReceivedMessage(string topic, string message, MqttApplicationMessageReceivedEventArgs rawArg)
        {
            base.OnReceivedMessage(topic, message, rawArg);

            if (topic == logTopic)
                OnLog?.Invoke(message);
            else if (topic == exceptionTopic)
                OnException?.Invoke(message);
            else if (topic == controlResponseTopic)
                OnControlResponse?.Invoke(message);
        }

        public Task SendCommand(string command)
        {
            return MqttClient?.PublishAsync(controlTopic, command, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
        }
    }
}