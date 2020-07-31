using LogMe.Core;
using System.Threading.Tasks;

namespace LogMeLib
{
    internal interface IWorkerClient : IClient
    {
        Task SendAsync(string message, MessageType messageType);
    }
}