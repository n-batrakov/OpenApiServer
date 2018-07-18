using Microsoft.Extensions.CommandLineUtils;

namespace ITExpert.OpenApi.Tools.Commands.Server
{
    public static class LaunchServerConfiguration
    {
        public static void Configure(CommandLineApplication cmd)
        {
            cmd.Description = "Launches OpenAPI server with SwaggerUI and mocking feature.";
            cmd.HelpOption("--help");

            var rootArg = cmd.Argument("[source]", "Input directory");
            var portArg = cmd.Option("-p|--port",
                                     "Specifies port listened by the server",
                                     CommandOptionType.SingleValue);
            var verbosityArg = cmd.Option("-v|--verbose",
                                          "If set server writes logs to STDOUT.",
                                          CommandOptionType.SingleValue);

            cmd.OnExecute(() =>
                          {
                              var options = new LaunchServerCommandOptions
                                            {
                                                    Port = int.TryParse(portArg.Value(), out var port) ? port : default,
                                                    SpecsDirectory = rootArg.Value,
                                                    Verbose = verbosityArg.HasValue()
                                            };
                              var command = new LaunchServerCommand(options);
                              return command.Execute();
                          });
        }
    }
}