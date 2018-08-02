using System;
using System.IO;
using System.Net.Http;

using ITExpert.OpenApi.Server.Configuration;
using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.DocumentProviders;
using ITExpert.OpenApi.Tools.Commands.Server.DocumentProviders;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ITExpert.OpenApi.Tools.Commands.Server
{
    public class LaunchServerCommand
    {
        private int Port { get; }
        private LogLevel LogLevel { get; }

        private string ConfigFile { get; }
        private string[] Sources { get; }
        private bool TreatSourcesAsDiscoveryFiles { get; }
        private string DiscoveryKey { get; }

        public LaunchServerCommand(LaunchServerCommandOptions options)
        {
            Port = options.Port;
            Sources = options.Sources;
            LogLevel = options.MinLogLevel;
            TreatSourcesAsDiscoveryFiles = options.TreatSourcesAsDiscoveryFiles;
            DiscoveryKey = options.DiscoveryKey;
            ConfigFile = options.ConfigPath;
        }

        public int Execute()
        {
            CreateConfig();

            var host = OpenApi.Server.Program
                              .CreateHostBuilder()
                              .UseStartup<Startup>()
                              .ConfigureAppConfiguration(ConfigureConfiguration)
                              .ConfigureServices(ConfigureServices)
                              .ConfigureLogging(ConfigureLogging)
                              .UseUrls($"http://*:{Port}")
                              .CaptureStartupErrors(false)
                              .SuppressStatusMessages(suppressStatusMessages: true)
                              .Build();
            try
            {
                host.Start();
            }
            catch (Exception e)
            {
                PrintStartupError(e);
                return 1;
            }
            
            PrintStartupMessage();
            host.WaitForShutdown();
            PrintFinish();

            return 0;
        }

        private void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.AddFilter("Microsoft.AspNetCore.Hosting.Internal.WebHost", LogLevel.None);
            logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Error);
            logging.SetMinimumLevel(LogLevel);
            
        }

        private void ConfigureConfiguration(IConfigurationBuilder config)
        {
            config.AddJsonFile(ConfigFile, optional: true, reloadOnChange: true);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IOpenApiDocumentProvider>(
                    x => new CliOpenApiDocumentProvider(Sources,
                                                        TreatSourcesAsDiscoveryFiles,
                                                        DiscoveryKey,
                                                        x.GetRequiredService<IHttpClientFactory>(),
                                                        x.GetRequiredService<ILoggerFactory>()));
        }

        private void PrintStartupMessage()
        {
            Console.WriteLine();
            Console.WriteLine($"OpenAPI Server is running on http://localhost:{Port}".PadRight(60));
            Console.WriteLine("Press Ctrl+C to terminate.".PadRight(60));

            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine($"* Verbosity: {LogLevel}");
            Console.WriteLine($"* Config: {Path.GetFullPath(ConfigFile)}");
            Console.WriteLine($"* Sources: {string.Join(", ", Sources)}");
            Console.WriteLine("".PadRight(60, '*'));
            Console.WriteLine();
        }

        private static void PrintStartupError(Exception e)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Critical startup error:");
            Console.ResetColor();
            Console.WriteLine(e.Message);
            Console.WriteLine();
            Console.WriteLine("Exiting...");
        }

        private static void PrintFinish()
        {
            Console.WriteLine();
            Console.WriteLine("Terminated.");
            Console.WriteLine();
        }

        private void CreateConfig()
        {
            if (File.Exists(ConfigFile))
            {
                return;
            }

            var dir = Path.GetDirectoryName(ConfigFile);
            Directory.CreateDirectory(dir);

            var optionsText = GetDefaultOptions();
            using (var writer = File.CreateText(ConfigFile))
            {
                writer.Write(optionsText);
            }
        }

        private string GetDefaultOptions()
        {
            var settings = new JsonSerializerSettings
                           {
                                   Formatting = Formatting.Indented,
                                   ContractResolver = new CamelCasePropertyNamesContractResolver(),
                           };
            settings.Converters.Add(new StringEnumConverter(camelCaseText: true));

            var options = new MockServerOptions
                          {
                                  MockServerHost = $"http://localhost:{Port}",
                                  Routes = new[] { MockServerRouteOptions.Default }
                          };
            return JsonConvert.SerializeObject(options, settings);
        }
    }
}