using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Utils
{
    public static class OpenApiDocumentExtensions
    {
        public static string GetId(this OpenApiDocument doc) =>
                $"{doc.Info.GetServiceName().ToLowerInvariant()}@{GetMajorVersion(doc.Info)}";

        public static string GetServiceName(this OpenApiInfo info) => info.Title.Replace(" ", "");

        public static string GetMajorVersion(this OpenApiInfo info)
        {
            var split = info.Version.Split('.');
            return split[0];
        }
    }
}