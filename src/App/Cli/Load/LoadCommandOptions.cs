namespace ITExpert.OpenApi.Server.Cli.Load
{
    public class LoadCommandOptions
    {
        public string[] Sources { get; set; }
        public string OutputPath { get; set; }
        public bool TreatSourcesAsDiscoveryFiles { get; set; }
        public string DiscoveryKey { get; set; }
    }
}