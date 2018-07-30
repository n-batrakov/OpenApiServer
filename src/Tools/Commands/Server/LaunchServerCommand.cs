using System.Net.Http;

using ITExpert.OpenApi.Server.Configuration;
using ITExpert.OpenApi.Server.Utils;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ITExpert.OpenApi.Tools.Commands.Server
{
    public class LaunchServerCommand
    {
        private int Port { get; }
        private LogLevel LogLevel { get; }

        private string ConfigFile { get; }
        private string[] Sources { get; }
        private bool TreatSourcesAsDiscoveryFiles { get; }
        private string DiscovertyKey { get; }

        public LaunchServerCommand(LaunchServerCommandOptions options)
        {
            Port = options.Port;
            Sources = options.Sources;
            LogLevel = options.MinLogLevel;
            TreatSourcesAsDiscoveryFiles = options.TreatSourcesAsDiscoveryFiles;
            DiscovertyKey = options.DiscoveryKey;
            ConfigFile = options.ConfigPath;
        }

        public int Execute()
        {
            OpenApi.Server.Program
                   .CreateHostBuilder()
                   .UseStartup<Startup>()
                   .ConfigureAppConfiguration(ConfigureConfiguration)
                   .ConfigureServices(ConfigureServices)
                   .ConfigureLogging(ConfigureLogging)
                   .UseUrls($"http://*:{Port}")
                   .Build()
                   .Run();
            return 0;
        }

        private void ConfigureLogging(ILoggingBuilder logging)
        {
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
                                                        DiscovertyKey,
                                                        x.GetRequiredService<IHttpClientFactory>()));
        }
    }
}