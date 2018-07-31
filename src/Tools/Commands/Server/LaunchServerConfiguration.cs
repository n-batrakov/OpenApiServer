using System.Linq;

using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace ITExpert.OpenApi.Tools.Commands.Server
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
                                         "Minimal log level; one of `trace`, `debug`, `information`, `warning`, `error`, `critical`, `none`.",
                                         CommandOptionType.SingleValue);
            var config = cmd.Option("-c|--config",
                                    "Path to config file.",
                                    CommandOptionType.SingleValue);
            var discrover = cmd.Option("-D|--discover",
                                       "If set, `source` argument treated as discovery file.",
                                       CommandOptionType.NoValue);
            var discoverKey = cmd.Option("--discover-key",
                                         "If discovery file is an object, this parameter sets the name of the property used for discovery.",
                                         CommandOptionType.SingleValue);

            cmd.OnExecute(() => new LaunchServerCommand(
                                  new LaunchServerCommandOptions
                                  {
                                          Port = port.GetIntValue(5000),
                                          Sources = sources.GetStringValues(".").ToArray(),
                                          MinLogLevel = minLogLevel.GetEnumValue(LogLevel.Information),
                                          ConfigPath = config.GetStringValue("./oas/oas.config.json"),
                                          TreatSourcesAsDiscoveryFiles = discrover.GetBooleanValue(),
                                          DiscoveryKey = discoverKey.GetStringValue()
                                  }).Execute());
        }
    }
}