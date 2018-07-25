using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Utils;

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.PathProviders
{
    public class ConfigOperationPathProvider : IOpenApiOperationPathProvider
    {
        private string PathFormatString { get; }

        private bool UseDefault { get; }

        public ConfigOperationPathProvider(IOptions<MockServerOptions> options)
        {
            PathFormatString = options.Value.PathPattern?.ToLowerInvariant();
            UseDefault = string.IsNullOrEmpty(options.Value.PathPattern);
        }

        public string GetPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath)
        {
            if (UseDefault)
            {
                return GetDefaultValue(spec, operation, operationPath);
            }

            var service = spec.Info.Title.Replace(" ", "");
            var path = operationPath.Substring(1);
            var version = spec.Info.GetMajorVersion();

            return Format(PathFormatString, ("service", service), ("path", path), ("version", version));
        }

        private static string GetDefaultValue(OpenApiDocument spec, OpenApiOperation operation, string operationPath) =>
                DefaultOperationPathProvider.GetDefaultPath(spec, operation, operationPath);

        private static string Format(string str, params (string, object)[] args)
        {
            foreach (var (name, value) in args)
            {
                str = str.Replace($"{{{name}}}", value.ToString());
            }

            return str;
        }
    }
}