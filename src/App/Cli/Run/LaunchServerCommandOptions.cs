using Microsoft.Extensions.Logging;

namespace ITExpert.OpenApi.Server.Cli.Run
{
    public class LaunchServerCommandOptions
    {
        public string[] Sources { get; set; }

        public int Port { get; set; }
        public LogLevel MinLogLevel { get; set; }
        public bool TreatSourcesAsDiscoveryFiles { get; set; }
        public string DiscoveryKey { get; set; }
        public string ConfigPath { get; set; }
    }
}