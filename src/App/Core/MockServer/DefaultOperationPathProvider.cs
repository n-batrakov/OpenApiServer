using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
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
            var version = $"/{spec.Info.GetMajorVersion()}";
            return $"{service}{version}{path}";
        }
    }
}