using System;
using System.Threading.Tasks;

namespace LogMe.Supervisor
{
    public interface ISupervisorClient
    {
        Action<string> OnLog { get; set; }
        Action<string> OnException { get; set; }
        Action<string> OnControlResponse { get; set; }

        Task StartAsync();

        Task StopAsync();

        Task SendCommand(string command);
    }
}