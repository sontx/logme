using LogMe.Core;
using System.Threading.Tasks;

namespace LogMe
{
    internal interface IWorkerClient : IClient
    {
        Task SendAsync(string message, MessageType messageType);
    }
}