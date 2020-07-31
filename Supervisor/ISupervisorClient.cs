using LogMe.Core;
using System;
using System.Threading.Tasks;

namespace LogMe.Supervisor
{
    public interface ISupervisorClient : IClient
    {
        Action<string> OnLog { get; set; }
        Action<string> OnException { get; set; }
        Action<string> OnControlResponse { get; set; }

        Task SendCommand(string command);
    }
}