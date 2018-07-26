using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Options;

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.PathProviders
{
    public class ConfigOperationPathProvider : IOpenApiOperationPathProvider
    {
        private string PathFormatString { get; }

        private bool ConfigHostNotSet { get; }

        public ConfigOperationPathProvider(IOptions<MockServerOptions> options)
        {
            PathFormatString = options.Value.MockServerHost;
            ConfigHostNotSet = string.IsNullOrEmpty(PathFormatString);
        }

        public string GetPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath)
        {
            var server = operation.Servers.FirstOrDefault() ?? spec.Servers.FirstOrDefault();
            var pathString = server == null ? GetPathFromConfig(spec, operation) : server.Url;
            var pathPrefix = UrlHelper.GetPathPrefix(pathString).ToLowerInvariant();

            var path = ConcatPathSegments(pathPrefix, operationPath);
            return $"/{path}/";
        }

        private static string ConcatPathSegments(params string[] segments) =>
                string.Join("/", segments.Where(x => x != "/").Select(x => x.Trim('/')));

        private string GetPathFromConfig(OpenApiDocument spec, OpenApiOperation operation)
        {
            return ConfigHostNotSet ? GetDefaultValue(spec, operation, "") : PathFormatString;
        }

        private static string GetDefaultValue(OpenApiDocument spec, OpenApiOperation operation, string operationPath) =>
                DefaultOperationPathProvider.GetDefaultPath(spec, operation, operationPath);
    }
}