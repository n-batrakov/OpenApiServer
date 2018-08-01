using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.DocumentProviders;
using ITExpert.OpenApi.Server.Utils;
using ITExpert.OpenApi.Tools.Commands.Server.DocumentProviders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Tools.Commands.Load
{
    public class LoadCommand
    {
        private LoadCommandOptions Options { get; }
        private ILogger Logger { get; }
        private IHttpClientFactory ClientFactory { get; }

        public LoadCommand(LoadCommandOptions options)
        {
            Options = options;
            ClientFactory = new HttpClientFactory();
            Logger = new ConsoleLogger("SpecLoader", (s, level) => true, includeScopes: false);
        }

        public int Execute()
        {
            if (Options.Sources == null || Options.Sources.Length == 0)
            {
                PrintArgumentError();
                return 1;
            }

            PrintStart();

            var specs = Options.Sources.SelectMany(GetSpecs);
            var tasks = specs.Select(WriteSpec).ToArray();
            Task.WaitAll(tasks);

            PrintFinish();
            return 0;
        }

        private IEnumerable<OpenApiDocument> GetSpecs(string source)
        {
            var provider = GetProvider(source);
            return provider.GetDocuments();
            
            IOpenApiDocumentProvider GetProvider(string url)
            {
                if (Options.TreatSourcesAsDiscoveryFiles)
                {
                    return new DiscoveryOpenApiDocumentProvider(ClientFactory, url, Options.DiscoveryKey, Logger);
                }
                else
                {
                    return new WebOpenApiDocumentProvider(ClientFactory, url);
                }
            }
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Success!");
            Console.ResetColor();
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