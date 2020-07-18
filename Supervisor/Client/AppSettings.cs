using Config4Net.Core;

namespace Client
{
    [Config("app")]
    internal class AppSettings
    {
        public string AppName { get; set; }
        public string ServerAddress { get; set; }
    }
}