using System.Collections.Generic;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Utils
{
    public interface IOpenApiDocumentProvider
    {
        IEnumerable<OpenApiDocument> GetDocuments();
    }
}