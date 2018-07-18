using Microsoft.Extensions.CommandLineUtils;

namespace ITExpert.OpenApi.Tools.Commands.Merge
{
    public class MergeConfiguration
    {
        public static void Configure(CommandLineApplication cmd)
        {
            cmd.Description =
                    "Merges multiple OpenApi documents (yml or json) with same title and version into one document";
            cmd.HelpOption("--help");


            var rootArg = cmd.Argument("[source]", "Input directory");
            var outputArg = cmd.Option("-o|--output",
                                       "Output directory",
                                       CommandOptionType.SingleValue);
            var formatArg = cmd.Option("-f|--format",
                                       "Output format. Must be `json` or `yaml`",
                                       CommandOptionType.SingleValue);
            var versionArg = cmd.Option("-v|--version",
                                        "Output OAS version. Must be `2` or `3`.",
                                        CommandOptionType.SingleValue);
            var recursriveArg = cmd.Option("-r|--recursvie",
                                           "Search for files in source directory recursivly.",
                                           CommandOptionType.NoValue);
            var flatOutputArg = cmd.Option("--flat",
                                           "Write files to output directory without creating subdirectories",
                                           CommandOptionType.NoValue);
            cmd.OnExecute(() =>
            {
                var options = new MergeOptions
                {
                    Format = formatArg.Value(),
                    Root = rootArg.Value,
                    Output = outputArg.Value(),
                    Version = versionArg.Value(),
                    Recursive = recursriveArg.HasValue(),
                    Flat = flatOutputArg.HasValue()
                };
                var command = new MergeCommand(options);
                return command.Execute();
            });
        }
    }
}