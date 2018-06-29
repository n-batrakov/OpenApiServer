using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.Extensions
{
    public static class OpenApiDocumentExtensions
    {
        public static string GetId(this OpenApiDocument doc) =>
                $"{doc.Info.Title.ToLowerInvariant()}@{doc.Info.Version}";
    }
}