using System;
using System.IO;
using System.Net.Http;

using ITExpert.OpenApi.Cli.Run;
using ITExpert.OpenApi.DocumentProviders;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ITExpert.OpenApi.Server
{
    public class WebHostFactory
    {
        private int Port { get; }
        private ServerVerbosityLevel VerbosityLevel { get; }
        private string ConfigFile { get; }
        private string[] Sources { get; }
        private bool TreatSourcesAsDiscoveryFiles { get; }
        private string DiscoveryKey { get; }

        public WebHostFactory(LaunchServerCommandOptions options)
        {
            Port = options.Port;
            Sources = options.Sources;
            VerbosityLevel = options.VerbosityLevel;
            TreatSourcesAsDiscoveryFiles = options.TreatSourcesAsDiscoveryFiles;
            DiscoveryKey = options.DiscoveryKey;
            ConfigFile = options.ConfigPath;
        }

        public IWebHost CreateHost()
        {
            var builder = new WebHostBuilder();

            builder
                    .UseKestrel(x => x.Configure())
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration(ConfigureConfiguration)
                    .UseDefaultServiceProvider(ConfigureServiceProvider)
                    .ConfigureServices(ConfigureServices)
                    .ConfigureLogging(ConfigureLogging)
                    .UseUrls($"http://*:{Port}")
                    .CaptureStartupErrors(false)
                    .SuppressStatusMessages(true)
                    .UseStartup<Startup>();

            return builder.Build();
        }

        private void ConfigureServiceProvider(WebHostBuilderContext ctx, ServiceProviderOptions options)
        {
            options.ValidateScopes = ctx.HostingEnvironment.IsDevelopment();
        }

        private void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.AddConsole();
            logging.SetMinimumLevel(GetLogLevel());

            // Suppress especially chatty services
            if (VerbosityLevel > ServerVerbosityLevel.Normal)
            {
                logging.AddFilter("Microsoft.AspNetCore.Hosting.Internal.WebHost", LogLevel.Error);
                logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer", LogLevel.Error);
                logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Error);
            }
            

            LogLevel GetLogLevel()
            {
                switch (VerbosityLevel)
                {
                    case ServerVerbosityLevel.Quiet:
                        return LogLevel.None;
                    case ServerVerbosityLevel.Minimal:
                        return LogLevel.Warning;
                    case ServerVerbosityLevel.Normal:
                        return LogLevel.Information;
                    case ServerVerbosityLevel.Detailed:
                        return LogLevel.Debug;
                    case ServerVerbosityLevel.Diagnostic:
                        return LogLevel.Trace;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ConfigureConfiguration(IConfigurationBuilder config)
        {
            config.AddJsonFile(ConfigFile, optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables();
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
    }
}