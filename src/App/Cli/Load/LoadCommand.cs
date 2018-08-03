using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ITExpert.OpenApi.DocumentProviders;
using ITExpert.OpenApi.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Cli.Load
{
    public class LoadCommand
    {
        private LoadCommandOptions Options { get; }
        private ILoggerFactory LoggerFactory { get; }
        private IHttpClientFactory ClientFactory { get; }

        public LoadCommand(LoadCommandOptions options)
        {
            Options = options;
            ClientFactory = new HttpClientFactory();

            LoggerFactory = new LoggerFactory(new[] {new ConsoleLoggerProvider(new ConsoleLoggerSettings())});
        }

        public int Execute()
        {
            if (Options.Sources == null || Options.Sources.Length == 0)
            {
                PrintArgumentError();
                return 1;
            }

            PrintStart();

            var provider = new CliOpenApiDocumentProvider(Options.Sources,
                                                          Options.TreatSourcesAsDiscoveryFiles,
                                                          Options.DiscoveryKey,
                                                          ClientFactory,
                                                          LoggerFactory);
            var specs = provider.GetDocuments().ToArray();

            if (specs.Length == 0)
            {
                PrintNoSpecs();
                return 0;
            }

            var tasks = specs.Select(WriteSpec).ToArray();
            Task.WaitAll(tasks);

            PrintFinish();
            return 0;
        }

        private async Task WriteSpec(OpenApiDocument spec)
        {
            Directory.CreateDirectory(Options.OutputPath);

            var filename = $"{spec.Info.GetServiceName()}.v{spec.Info.GetMajorVersion()}.json";
            var path = Path.Join(Options.OutputPath, filename);
            var text = OpenApiSerializer.Serialize(spec);

            using (var writer = File.CreateText(path))
            {
                await writer.WriteAsync(text).ConfigureAwait(false);
            }
        }

        private void PrintStart()
        {
            Console.WriteLine();
            Console.WriteLine("OpenAPI specification loading is launched.");

            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine($"* Sources: {string.Join(", ", Options.Sources)}");
            Console.WriteLine($"* Output directory: {Path.GetFullPath(Options.OutputPath)}");
            Console.WriteLine(
                    $"* Discovery: {(Options.TreatSourcesAsDiscoveryFiles ? "Yes" : "No")} (key: '{Options.DiscoveryKey}')");
            
            Console.WriteLine();

            Console.WriteLine("Stand by...");
        }

        private static void PrintFinish()
        {
            Console.WriteLine();
            Console.WriteLine("Complete.");
            Console.WriteLine();
        }

        private static void PrintNoSpecs()
        {
            Console.WriteLine();
            Console.WriteLine("No specs were loaded");
            Console.WriteLine("Make sure specified sources contain valid OpenApi specifications.");
            Console.WriteLine("If discovery endpoint is used, don't forget to add '-D' key.");
            Console.WriteLine();
        }

        private static void PrintArgumentError()
        {
            Console.WriteLine("Load command requires at least one argument to be specified.");
            Console.WriteLine("See 'oas load --help'.");
        }

        private class HttpClientFactory : IHttpClientFactory
        {
            private HttpClient Client { get; }

            public HttpClientFactory()
            {
                Client = new HttpClient();
            }

            public HttpClient CreateClient(string name) => Client;
        }
    }
}