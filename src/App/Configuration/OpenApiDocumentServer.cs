using System.Collections.Generic;
using System.IO;

using ITExpert.OpenApiServer.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.Configuration
{
    public static class OpenApiDocumentServer
    {
        public static IApplicationBuilder UseOpenApiServer(this IApplicationBuilder app,
                                                           IEnumerable<OpenApiDocument> specs,
                                                           string webRootPath)
        {
            WriteSpecs(webRootPath, specs);

            return app.UseFileServer(new FileServerOptions
                                     {
                                             EnableDirectoryBrowsing = true,
                                             DirectoryBrowserOptions = {RequestPath = ""}
                                     });
        }

        private static void WriteSpecs(string dir, IEnumerable<OpenApiDocument> specs)
        {
            foreach (var spec in specs)
            {
                WriteOneSpec(spec);
            }

            void WriteOneSpec(OpenApiDocument spec)
            {
                var path = GetSpecFilePath(dir, spec.Info.GetMajorVersion(), spec.Info.Title);
                var content = spec.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var writer = File.CreateText(path))
                {
                    writer.Write(content);
                }
            }
        }

        private static string GetSpecFilePath(string dir, string apiVersion, string apiTitle)
        {
            const string filename = "swagger.json";
            var ver = $"v{apiVersion}";
            var name = apiTitle.Replace(" ", "");
            return Path.Combine(dir, name, ver, filename);
        }
    }
}