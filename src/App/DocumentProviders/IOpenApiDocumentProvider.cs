using System.Collections.Generic;

using Microsoft.OpenApi.Models;

namespace OpenApiServer.DocumentProviders
{
    public interface IOpenApiDocumentProvider
    {
        IEnumerable<OpenApiDocument> GetDocuments();
    }
}