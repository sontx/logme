using System;
using System.Threading.Tasks;

namespace Client
{
    public interface ISupervisorClient
    {
        Action<string> OnMessage { get; set; }

        Task StartAsync();

        Task StopAsync();
    }
}
