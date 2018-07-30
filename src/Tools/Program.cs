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
                              Description = "Tools for OpenAPI specification."
                      };
            app.HelpOption("--help");
            app.Command("merge", MergeConfiguration.Configure);
            app.Command("run", LaunchServerConfiguration.Configure);

            if (args == null || args.Length == 0)
            {
                args = new[] { "run" };
            }

            return app.Execute(args);
        }
    }
}