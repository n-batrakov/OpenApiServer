using Microsoft.Extensions.CommandLineUtils;

namespace ITExpert.OpenApi.Server.Cli.Load
{
    public static class LoadCommandConfiguration
    {
        public static void Configure(CommandLineApplication cmd)
        {
            cmd.Description = "Loads OpenAPI specification files from specified URLs.";
            cmd.HelpOption("--help");

            var sources = cmd.Argument("[sources]",
                                       "URLs with OpenAPI specification documents.",
                                       multipleValues: true);
            var output = cmd.Option("-o|--output", 
                                    "Directory to put downloaded specs in.",
                                    CommandOptionType.SingleValue);
            var discover = cmd.Option("-D|--discover",
                                      "If set, `source` argument treated as discovery file.",
                                      CommandOptionType.NoValue);
            var discoverKey = cmd.Option("-k|--discover-key",
                                         "If discovery file is an object, this parameter sets the name of the property used for discovery.",
                                         CommandOptionType.SingleValue);

            cmd.OnExecute(() => new LoadCommand(new LoadCommandOptions
                                                {
                                                        Sources = sources.Values.ToArray(),
                                                        TreatSourcesAsDiscoveryFiles = discover.GetBooleanValue(),
                                                        DiscoveryKey = discoverKey.GetStringValue(),
                                                        OutputPath = output.GetStringValue("./.oas/specs")
                                                }).Execute());
        }
    }
}