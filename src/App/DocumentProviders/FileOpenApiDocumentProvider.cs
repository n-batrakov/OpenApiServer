using System.Collections.Generic;
using System.IO;

using Microsoft.OpenApi.Models;

using OpenApiServer.Utils;

namespace OpenApiServer.DocumentProviders
{
    public class FileOpenApiDocumentProvider : IOpenApiDocumentProvider
    {
        private string Uri { get; }

        public FileOpenApiDocumentProvider(string uri)
        {
            Uri = uri;
        }

        public IEnumerable<OpenApiDocument> GetDocuments()
        {
            var text = File.ReadAllText(Uri);
            yield return OpenApiDocumentUtils.ReadSpec(text);
        }
    }
}