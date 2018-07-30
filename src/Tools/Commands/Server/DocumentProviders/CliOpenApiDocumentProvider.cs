using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

using ITExpert.OpenApi.Server.Utils;
using ITExpert.OpenApi.Tools.Commands.Server.DocumentProviders;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Tools.Commands.Server
{
    public class CliOpenApiDocumentProvider : IOpenApiDocumentProvider
    {
        private IHttpClientFactory ClientFactory { get; }
        private IOpenApiDocumentProvider[] Providers { get; }

        public CliOpenApiDocumentProvider(IEnumerable<string> uris,
                                          bool treatUriAsDiscovery,
                                          string discoveryKey,
                                          IHttpClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
            Providers = GetProviders(uris, treatUriAsDiscovery, discoveryKey).ToArray();
        }

        public IEnumerable<OpenApiDocument> GetDocuments() =>
                Providers.SelectMany(x => x.GetDocuments()).Where(x => x != null).ToArray();


        private IEnumerable<IOpenApiDocumentProvider> GetProviders(IEnumerable<string> uris, bool treatUriAsDiscovery, string discoveryKey)
        {
            return treatUriAsDiscovery
                           ? uris.Select(x => new DiscoveryOpenApiDocumentProvider(ClientFactory, x, discoveryKey))
                           : uris.Select(x => GetProvider(GetSourceType(x), x));
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

        private static DocumentSourceType GetSourceType(string uri)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;
            if (uri.StartsWith("http://", comparison) || uri.StartsWith("https://", comparison))
            {
                return DocumentSourceType.Web;
            }

            if (Directory.Exists(uri))
            {
                return DocumentSourceType.Directory;
            }

            if (File.Exists(uri))
            {
                return DocumentSourceType.File;
            }

            throw new ArgumentException(
                    $"Unable to determine type for given URI ('{uri}'). " +
                    "If it is file or directory, make sure it exists. " +
                    "In case of web URI make sure protocol specified " +
                    "correctly (only http(-s) is supported.");
        }

        private enum DocumentSourceType
        {
            File,
            Directory,
            Web
        }
    }
}