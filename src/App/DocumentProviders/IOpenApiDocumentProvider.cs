using System.Collections.Generic;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.DocumentProviders
{
    public interface IOpenApiDocumentProvider
    {
        IEnumerable<OpenApiDocument> GetDocuments();
    }
}