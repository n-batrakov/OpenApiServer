using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using ITExpert.OpenApi.Server.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Tools.Commands.Server.DocumentProviders
{
    public class CliOpenApiDocumentProvider : IOpenApiDocumentProvider
    {
        private IHttpClientFactory ClientFactory { get; }
        private IOpenApiDocumentProvider[] Providers { get; }
        private ILogger Logger { get; }

        public CliOpenApiDocumentProvider(IEnumerable<string> uris,
                                          bool treatUriAsDiscovery,
                                          string discoveryKey,
                                          IHttpClientFactory clientFactory,
                                          ILoggerFactory loggerFactory)
        {
            ClientFactory = clientFactory;
            Logger = loggerFactory.CreateLogger("DocumentProvider");
            Providers = GetProviders(uris, treatUriAsDiscovery, discoveryKey, Logger).ToArray();
        }

        public IEnumerable<OpenApiDocument> GetDocuments()
        {
            var specs = Providers.SelectMany(x => x.GetDocuments()).Where(x => x != null).ToArray();
            LogLoadedSpecs(specs);
            return specs;
        }

        private IEnumerable<IOpenApiDocumentProvider> GetProviders(IEnumerable<string> uris,
                                                                   bool treatUriAsDiscovery,
                                                                   string discoveryKey,
                                                                   ILogger logger)
        {
            return treatUriAsDiscovery
                           ? uris.Select(
                                   x => new DiscoveryOpenApiDocumentProvider(ClientFactory, x, discoveryKey, logger))
                           : uris.Select(x => GetProvider(DocumentSourceTypeProvider.GetSourceType(x), x));
        }

        private IOpenApiDocumentProvider GetProvider(DocumentSourceType type, string uri)
        {
            switch (type)
            {
                case DocumentSourceType.File:
                    return new FileOpenApiDocumentProvider(uri);
                case DocumentSourceType.Directory:
                    return new OpenApiDocumentsProvider(uri);
                case DocumentSourceType.Web:
                    return new WebOpenApiDocumentProvider(ClientFactory, uri);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void LogLoadedSpecs(IEnumerable<OpenApiDocument> specs)
        {
            var loadedSpecs = string.Join(", ", specs.Select(x => $"{x.Info.Title} (v{x.Info.Version})"));
            var msg = $"Specs loaded: {loadedSpecs}";
            Logger.LogInformation(msg);
        }
    }
}