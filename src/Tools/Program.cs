using System;

using ITExpert.OpenApi.Tools.Commands.Load;
using ITExpert.OpenApi.Tools.Commands.Merge;
using ITExpert.OpenApi.Tools.Commands.Server;

using Microsoft.Extensions.CommandLineUtils;

namespace ITExpert.OpenApi.Tools
{
    public static class Program
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
            app.VersionOption("-v|--version", "0.1.0");

            app.Command("merge", MergeConfiguration.Configure);
            app.Command("run", LaunchServerConfiguration.Configure);
            app.Command("load", LoadCommandConfiguration.Configure);

            if (args.Length == 0)
            {
                app.ShowHelp();
                return -1;
            }

            app.ShowRootCommandFullNameAndVersion();

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                app.ShowHelp();
                return -1;
            }
        }
    }
}