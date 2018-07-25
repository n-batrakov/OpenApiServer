using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.PathProviders
{
    public class DefaultOperationPathProvider : IOpenApiOperationPathProvider
    {
        public string GetPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath)
        {
            return GetDefaultPath(spec, operation, operationPath);
        }

        internal static string GetDefaultPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath)
        {
            var service = spec.Info.Title.Replace(" ", "");
            var path = operationPath;
            var version = $"/v{spec.Info.GetMajorVersion()}";
            return $"{service}{version}{path}";
        }
    }
}