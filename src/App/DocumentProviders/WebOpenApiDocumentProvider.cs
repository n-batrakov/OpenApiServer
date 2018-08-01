using System.Collections.Generic;
using System.Net.Http;

using ITExpert.OpenApi.Server.Utils;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.DocumentProviders
{
    public class WebOpenApiDocumentProvider : IOpenApiDocumentProvider
    {
        private IHttpClientFactory ClientFactory { get; }
        private string Uri { get; }

        public WebOpenApiDocumentProvider(IHttpClientFactory clientFactory, string uri)
        {
            ClientFactory = clientFactory;
            Uri = uri;
        }

        public IEnumerable<OpenApiDocument> GetDocuments()
        {
            var client = ClientFactory.CreateClient();
            var spec = client.GetStringAsync(Uri).Result;
            yield return OpenApiDocumentUtils.ReadSpec(spec);
        }
    }
}