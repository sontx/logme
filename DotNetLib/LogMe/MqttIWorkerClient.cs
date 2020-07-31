using LogMe.Core;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogMeLib
{
    internal class MqttIWorkerClient : AbstractMqttClient, IWorkerClient
    {
        private readonly string pushLogTopic;
        private readonly string exceptionTopic;
        private readonly string controlTopic;
        private readonly string controlResponseTopic;
        private readonly Queue<PendingMessage> pendingMessageQueue = new Queue<PendingMessage>();

        public ICommandHandler CommandHandler { get; set; }

        public MqttIWorkerClient(string clientId, string url, string appName)
            : base(clientId, url)
        {
            pushLogTopic = string.Format(Constants.TOPIC_LOGS, appName);
            exceptionTopic = string.Format(Constants.TOPIC_EXCEPTIONS, appName);
            controlTopic = string.Format(Constants.TOPIC_CONTROLS, appName);
            controlResponseTopic = string.Format(Constants.TOPIC_CONTROLS_RESPONSE, appName);
        }

        protected async override void OnConnected()
        {
            base.OnConnected();
            await MqttClient.SubscribeAsync(new MqttTopicFilter
            {
                Topic = controlTopic,
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce
            });
            await SendPendingMessages(MqttClient);
        }

        protected override void OnReceivedMessage(string topic, string message, MqttApplicationMessageReceivedEventArgs rawArg)
        {
            base.OnReceivedMessage(topic, message, rawArg);
            CommandHandler?.HandleCommand(message);
        }

        public async Task SendAsync(string message, MessageType messageType)
        {
            var client = this.MqttClient;

            try
            {
                if (PrepareSending(message, messageType, client))
                {
                    await SendPendingMessages(client);
                    await SendImplAsync(message, messageType, client);
                }
            }
            catch (Exception e)
            {
                throw new LogMeException("Error while sending '" + message + "' to " + pushLogTopic, e);
            }
        }

        private bool PrepareSending(String message, MessageType messageType, IManagedMqttClient client)
        {
            bool ready = client != null && client.IsConnected;

            if (!ready)
            {
                pendingMessageQueue.Enqueue(new PendingMessage(message, messageType));
            }
            return ready;
        }

        private async Task SendPendingMessages(IManagedMqttClient client)
        {
            while (pendingMessageQueue.Count > 0)
            {
                var pendingMessage = pendingMessageQueue.Dequeue();
                await SendImplAsync(pendingMessage.Message, pendingMessage.MessageType, client);
            }
        }

        private Task SendImplAsync(string message, MessageType messageType, IManagedMqttClient client)
        {
            var topic = GetTopicByMessageType(messageType);
            return client.PublishAsync(topic, message, MqttQualityOfServiceLevel.AtMostOnce);
        }

        private string GetTopicByMessageType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Log:
                    return pushLogTopic;

                case MessageType.Exception:
                    return exceptionTopic;

                case MessageType.ControlResponse:
                    return controlResponseTopic;
            }
            return null;
        }

        private class PendingMessage
        {
            public string Message { get; }
            public MessageType MessageType { get; }

            public PendingMessage(string message, MessageType messageType)
            {
                this.Message = message;
                this.MessageType = messageType;
            }
        }
    }
}