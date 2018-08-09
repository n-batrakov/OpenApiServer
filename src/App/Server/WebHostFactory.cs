using System.IO;
using System.Net.Http;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenApiServer.Cli.Run;
using OpenApiServer.DocumentProviders;
using OpenApiServer.Server.Logging;

namespace OpenApiServer.Server
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
            logging.AddProvider(new CliLoggerProvider(VerbosityLevel));
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