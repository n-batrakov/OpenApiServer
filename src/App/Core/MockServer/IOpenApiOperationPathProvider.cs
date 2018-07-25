using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public interface IOpenApiOperationPathProvider
    {
        string GetPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath);
    }
}