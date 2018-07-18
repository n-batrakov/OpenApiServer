namespace ITExpert.OpenApi.Tools.Commands.Server
{
    public class LaunchServerCommandOptions
    {
        public int Port { get; set; }
        public string SpecsDirectory { get; set; }
        public bool Verbose { get; set; }
    }
}