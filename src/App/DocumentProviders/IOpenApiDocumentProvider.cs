using System.Collections.Generic;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.DocumentProviders
{
    public interface IOpenApiDocumentProvider
    {
        IEnumerable<OpenApiDocument> GetDocuments();
    }
}