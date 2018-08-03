namespace ITExpert.OpenApi.Cli.Run
{
    public enum ServerVerbosityLevel
    {
        Quiet,
        Minimal,
        Normal,
        Detailed,
        Diagnostic
    }

    public class LaunchServerCommandOptions
    {
        public string[] Sources { get; set; }

        public int Port { get; set; }
        public ServerVerbosityLevel VerbosityLevel { get; set; }
        public bool TreatSourcesAsDiscoveryFiles { get; set; }
        public string DiscoveryKey { get; set; }
        public string ConfigPath { get; set; }
    }
}