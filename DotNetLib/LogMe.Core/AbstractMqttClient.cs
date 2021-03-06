﻿using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogMe.Core
{
    public abstract class AbstractMqttClient : IClient, IConnectingFailedHandler, IMqttClientConnectedHandler, IMqttClientDisconnectedHandler
    {
        private readonly string clientId;
        private readonly string url;
        private readonly ManualResetEvent startWaitEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent stopWaitEvent = new ManualResetEvent(false);
        private string connectingFailedReason;
        private IManagedMqttClient mqttClient;

        protected IManagedMqttClient MqttClient => mqttClient;

        public AbstractMqttClient(string clientId, string url)
        {
            this.clientId = clientId;
            this.url = url.ToLower();
        }

        protected virtual void OnConnected()
        {
        }

        protected virtual void OnDisconnected()
        {
        }

        protected virtual void OnReceivedMessage(string topic, string message, MqttApplicationMessageReceivedEventArgs rawArg)
        {
        }

        public Task StartAsync()
        {
            connectingFailedReason = null;

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithCleanSession();
            if (url.StartsWith("ws://"))
            {
                var subUrl = url.Substring("ws://".Length);
                mqttClientOptions.WithWebSocketServer(subUrl);
            }
            else
            {
                var subUrl = url.StartsWith("tcp://") ? url.Substring("tcp://".Length) : url;
                mqttClientOptions.WithTcpServer(subUrl);
            }

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(mqttClientOptions.Build())
                .Build();

            return Task.Run(async () =>
            {
                if (this.mqttClient != null)
                    await this.mqttClient.StopAsync();
                var mqttClient = new MqttFactory().CreateManagedMqttClient();
                mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    var msg = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var topic = e.ApplicationMessage.Topic;
                    OnReceivedMessage(topic, msg, e);
                });
                mqttClient.ConnectedHandler = this;
                mqttClient.ConnectingFailedHandler = this;
                mqttClient.DisconnectedHandler = this;
                startWaitEvent.Reset();
                await mqttClient.StartAsync(options);
                startWaitEvent.WaitOne();
                if (!string.IsNullOrEmpty(connectingFailedReason))
                    throw new LogMeException(connectingFailedReason);
                this.mqttClient = mqttClient;
                OnConnected();
            });
        }

        public async Task StopAsync()
        {
            startWaitEvent?.Dispose();
            var mqttClient = this.mqttClient;
            if (mqttClient != null)
            {
                await mqttClient.StopAsync();
                stopWaitEvent.WaitOne();
                stopWaitEvent?.Dispose();
                this.mqttClient = null;
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
            OnDisconnected();
            return Task.Run(() =>
            {
                stopWaitEvent.Set();
            });
        }
    }
}