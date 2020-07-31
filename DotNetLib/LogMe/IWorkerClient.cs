using System;
using System.Threading.Tasks;

namespace LogMe
{
    internal interface IWorkerClient
    {
        Task StartAsync();

        Task StopAsync();

        void Send(string message, MessageType messageType);
    }
}