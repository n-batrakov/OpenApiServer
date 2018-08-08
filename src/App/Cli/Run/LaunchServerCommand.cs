using System;
using System.IO;

using ITExpert.OpenApi.Core.MockServer.Options;
using ITExpert.OpenApi.Server;
using ITExpert.OpenApi.Server.Logging;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ITExpert.OpenApi.Cli.Run
{
    public class LaunchServerCommand
    {
        private ILogger Logger { get; }
        private LaunchServerCommandOptions Options { get; }

        public LaunchServerCommand(LaunchServerCommandOptions options)
        {
            Options = options;
            Logger = new CliLoggerProvider(options.VerbosityLevel).CreateLogger(CliLoggerProvider.OpenApiLoggerName);
        }

        public int Execute()
        {
            LogStart();
            CreateConfig();

            var host = new WebHostFactory(Options).CreateHost();
            try
            {
                host.Start();
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "Unable to start application.");
                return 1;
            }
            
            Logger.LogInformation(ServerStartMessage);
            host.WaitForShutdown();
            Logger.LogInformation("Teminated.");

            return 0;
        }

        private void CreateConfig()
        {
            var path = Options.ConfigPath;
            if (File.Exists(path))
            {
                return;
            }

            var dir = Path.GetDirectoryName(path);
            if (dir != string.Empty)
            {
                Directory.CreateDirectory(dir);
            }

            var optionsText = GetDefaultOptions();
            using (var writer = File.CreateText(path))
            {
                writer.Write(optionsText);
            }

            string GetDefaultOptions()
            {
                var settings = new JsonSerializerSettings
                               {
                                       Formatting = Formatting.Indented,
                                       ContractResolver = new CamelCasePropertyNamesContractResolver(),
                               };
                settings.Converters.Add(new StringEnumConverter(camelCaseText: true));

                var options = new MockServerOptions
                              {
                                      MockServerHost = $"http://localhost:{Options.Port}",
                                      Routes = new[] {MockServerRouteOptions.Default}
                              };
                return JsonConvert.SerializeObject(options, settings);
            }
        }

        private string ServerStartMessage => $@"
OpenAPI Server is running on http://localhost:{Options.Port}
Press Ctrl+C to terminate.

Paramters:
* Verbosity: {Options.VerbosityLevel}
* Config: {Options.ConfigPath}
* Source: {string.Join(", ", Options.Sources)}
***************************************************************
";

        private void LogStart()
        {
            if (Options.VerbosityLevel == ServerVerbosityLevel.Quiet)
            {
                return;
            }

            // Logger not used because we want to write some startup message
            // when Verbosity is minimal (Warning log level).
            Console.WriteLine("OpenAPI Server is starting...");
        }
    }
}