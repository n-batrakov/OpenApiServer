using System;
using System.Linq;

using Microsoft.Extensions.CommandLineUtils;

namespace ITExpert.OpenApi.Server.Cli.Run
{
    public static class LaunchServerConfiguration
    {
        public static void Configure(CommandLineApplication cmd)
        {
            cmd.Description = "Launches OpenAPI server.";
            cmd.HelpOption("--help");

            var sources = cmd.Argument("[sources]",
                                       "Files | Directories | URLs with OpenAPI specification documents.",
                                       multipleValues: true);
            var port = cmd.Option("-p|--port",
                                  "Specifies port listened by the server",
                                  CommandOptionType.SingleValue);
            var minLogLevel = cmd.Option("-v|--verbosity",
                                         "Set the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].",
                                         CommandOptionType.SingleValue);
            var config = cmd.Option("-c|--config",
                                    "Path to config file.",
                                    CommandOptionType.SingleValue);
            var discover = cmd.Option("-D|--discover",
                                       "If set, `source` argument treated as discovery file.",
                                       CommandOptionType.NoValue);
            var discoverKey = cmd.Option("-k|--discover-key",
                                         "If discovery file is an object, this parameter sets the name of the property used for discovery.",
                                         CommandOptionType.SingleValue);

            cmd.OnExecute(() => new LaunchServerCommand(
                                  new LaunchServerCommandOptions
                                  {
                                          Port = port.GetIntValue(5000),
                                          Sources = sources.GetStringValues("./.oas/specs").ToArray(),
                                          VerbosityLevel = ParseVerbosityValue(minLogLevel.GetStringValue("normal")),
                                          ConfigPath = config.GetStringValue("./.oas/oas.config.json"),
                                          TreatSourcesAsDiscoveryFiles = discover.GetBooleanValue(),
                                          DiscoveryKey = discoverKey.GetStringValue()
                                  }).Execute());
        }

        private static ServerVerbosityLevel ParseVerbosityValue(string value)
        {
            if (Enum.TryParse<ServerVerbosityLevel>(value, ignoreCase:true, out var result))
            {
                return result;
            }

            var lowerValue = value.ToLowerInvariant();

            switch (lowerValue)
            {
                case "q":
                    return ServerVerbosityLevel.Quiet;
                case "m":
                    return ServerVerbosityLevel.Minimal;
                case "n":
                    return ServerVerbosityLevel.Normal;
                case "d":
                    return ServerVerbosityLevel.Detailed;
                case "diag":
                    return ServerVerbosityLevel.Diagnostic;
                default:
                    Console.WriteLine($"Unknown verbosity level ('{value}'). 'normal' used as a fallback.");
                    return ServerVerbosityLevel.Normal;
            }
        }
    }
}