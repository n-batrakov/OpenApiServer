using System.Collections.Generic;
using System.IO;

using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.DocumentProviders
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