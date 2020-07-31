using DeviceId;
using System;
using System.Threading.Tasks;

namespace LogMeLib
{
    public sealed class LogMe : ICommandHandler
    {
        public static async Task<LogMe> StartNewAsync(string url)
        {
            var logMe = new LogMe(url);
            await logMe.StartAsync();
            return logMe;
        }

        private readonly string url;
        private IWorkerClient workerClient;

        public LogMe(string url)
        {
            this.url = url;
        }

        public async Task StartAsync()
        {
            Logger.SetLogMe(this);

            try
            {
                if (workerClient != null)
                {
                    await workerClient.StopAsync();
                }

                var appName = SystemInfo.GetAppName();
                var clientId = GetClientId(appName);
                var mqttIWorkerClient = new MqttIWorkerClient(clientId, url, appName);
                mqttIWorkerClient.CommandHandler = this;
                await mqttIWorkerClient.StartAsync();
                workerClient = mqttIWorkerClient;
            }
            catch (Exception ex)
            {
                Logger.E("Error while starting " + GetType().Name, ex);
            }
        }

        private string GetClientId(string appName)
        {
            var deviceId = new DeviceIdBuilder()
                .AddMachineName()
                .AddMacAddress()
                .AddProcessorId()
                .AddMotherboardSerialNumber()
                .ToString();
            return $"{appName}@{deviceId}";
        }

        public async Task StopAsync()
        {
            Logger.SetLogMe(null);

            try
            {
                var workerClient = this.workerClient;
                if (workerClient != null)
                {
                    await workerClient.StopAsync();
                    this.workerClient = null;
                }
            }
            catch { }
        }

        public Task SendAsync(string log)
        {
            return SendImplAsync(log, MessageType.Log);
        }

        private async Task SendImplAsync(string msg, MessageType messageType)
        {
            try
            {
                var workerClient = this.workerClient;
                if (workerClient != null)
                {
                    await workerClient.SendAsync(msg, messageType);
                }
            }
            catch (Exception e)
            {
                Logger.E("Error while sending crash logs", e);
            }
        }

        public async void HandleCommand(string command)
        {
            string data = null;
            switch (command.ToUpper())
            {
                case Constants.COMMAND_GET_SYSTEM_INFO:
                    data = SystemInfo.GetInfo();
                    break;

                case Constants.COMMAND_TAKE_SCREENSHOT:
                    byte[] screenshot = ScreenshotHelper.TakeScreenshot();
                    data = Convert.ToBase64String(screenshot);
                    break;
            }

            if (!string.IsNullOrEmpty(data))
            {
                var appName = SystemInfo.GetAppName();
                var wrap = $"{command}\n{appName}\n{data}";
                await SendImplAsync(wrap, MessageType.ControlResponse);
            }
        }
    }
}