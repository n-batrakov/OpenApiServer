using System;

using ITExpert.OpenApi.Cli.Load;
using ITExpert.OpenApi.Cli.Merge;
using ITExpert.OpenApi.Cli.Run;

using Microsoft.Extensions.CommandLineUtils;

namespace ITExpert.OpenApi
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
                      {
                              Name = "oas",
                              FullName = "OpenAPI Server tools",
                              Description = "Tools for OpenAPI specification.",
                      };

            app.HelpOption("-h|--help");
            app.VersionOption("-v|--version", GetVersion);

            app.Command("merge", MergeConfiguration.Configure);
            app.Command("run", LaunchServerConfiguration.Configure);
            app.Command("load", LoadCommandConfiguration.Configure);


            if (args.Length == 0)
            {
                app.ShowHelp();
                return -1;
            }

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                return -1;
            }
        }

        private static string GetVersion() =>
                typeof(Program).Assembly.GetName().Version.ToString(3);
    }
}
