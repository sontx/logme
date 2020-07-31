using System.Threading.Tasks;

namespace LogMe.Core
{
    public interface IClient
    {
        Task StartAsync();

        Task StopAsync();
    }
}