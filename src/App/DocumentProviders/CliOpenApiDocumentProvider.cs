using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.DocumentProviders
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

            foreach (string uri in uris)
            {
                var uriType = DocumentSourceTypeProvider.GetSourceType(uri);
                if (uriType == DocumentSourceType.Unknown)
                {
                    LogUnknownUriType(uri);
                    continue;
                }

                yield return treatUriAsDiscovery
                                     ? new DiscoveryOpenApiDocumentProvider(ClientFactory, uri, discoveryKey, logger)
                                     : GetProvider(uriType, uri);
            }
        }

        private IOpenApiDocumentProvider GetProvider(DocumentSourceType type, string uri)
        {
            switch (type)
            {
                case DocumentSourceType.File:
                    return new FileOpenApiDocumentProvider(uri);
                case DocumentSourceType.Directory:
                    return new DirectoryOpenApiDocumentsProvider(uri);
                case DocumentSourceType.Web:
                    return new WebOpenApiDocumentProvider(ClientFactory, uri);
                case DocumentSourceType.Unknown:
                    throw new Exception("Unable to determine URI type.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void LogLoadedSpecs(IEnumerable<OpenApiDocument> specs)
        {
            var loadedSpecs = string.Join(", ", specs.Select(x => $"{x.Info.Title} (v{x.Info.Version})"));

            if (loadedSpecs == string.Empty)
            {
                var msg = "No specs were loaded";
                Logger.LogWarning(msg);
            }
            else
            {
                var msg = $"Specs loaded: {loadedSpecs}";
                Logger.LogInformation(msg);
            }
        }

        private void LogUnknownUriType(string uri)
        {
            const string msg = "Unable to determine type for given URI ('{0}'). " +
                               "If it is file or directory, make sure it exists. " +
                               "In case of web URI make sure protocol specified " +
                               "correctly (only http(-s) is supported).";
            Logger.LogWarning(msg, uri);
        }
    }
}