using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Server.Core.DocumentationServer
{
    public static class OpenApiDocumentServerExtensions
    {
        private const string SpecsUrl = "specs";
        private const string SpecFilename = "openapi.json";

        public static IApplicationBuilder UseOpenApiDocumentServer(this IApplicationBuilder app,
                                                                   IEnumerable<OpenApiDocument> specs,
                                                                   string host = "")
        {
            var routerBuilder = new RouteBuilder(app);
            var availableSpecs = new JArray();

            foreach (var spec in specs)
            {
                var route = $"/{GetSpecRoute(spec)}";
                var specString = OpenApiSerializer.Serialize(spec);

                routerBuilder.MapGet(route, x => x.Response.WriteJsonAsync(specString));
                availableSpecs.Add(GetAvailableSpecItem(spec, route, host));
            }

            routerBuilder.MapGet(SpecsUrl, x => x.Response.WriteJsonAsync(availableSpecs.ToString()));

            return app.UseRouter(routerBuilder.Build());
        }

        private static JObject GetAvailableSpecItem(OpenApiDocument x, string route, string host)
        {
            var name = $"{x.Info.Title}_{x.Info.GetMajorVersion()}";
            var url = UrlHelper.Join(host, route);
            return new JObject { { "name", name }, { "url", url } };
        }

        private static Task WriteJsonAsync(this HttpResponse response, string content)
        {
            response.ContentType = "application/json";
            return response.WriteAsync(content, Encoding.UTF8);
        }

        private static string GetSpecRoute(OpenApiDocument spec)
        {
            var ver = $"v{spec.Info.GetMajorVersion()}";
            var name = spec.Info.Title.Replace(" ", "").ToLowerInvariant();
            return $"{SpecsUrl}/{name}/{ver}/{SpecFilename}";
        }
    }
}