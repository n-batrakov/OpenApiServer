using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.Extensions
{
    public static class OpenApiDocumentExtensions
    {
        public static string GetId(this OpenApiDocument doc) =>
                $"{doc.Info.Title.ToLowerInvariant()}@{GetMajorVersion(doc.Info)}";

        public static string GetMajorVersion(this OpenApiInfo info)
        {
            var split = info.Version.Split('.');
            return split[0];
        }
    }
}