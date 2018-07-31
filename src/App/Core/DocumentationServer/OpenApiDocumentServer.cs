using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;

namespace ITExpert.OpenApi.Server.Core.DocumentationServer
{
    public static class OpenApiDocumentServer
    {
        private const string SpecsOutputDirectory = "out";
        private const string SpecFilename = "openapi.json";

        private const string SpecsRequestPath = "specs"; 

        private const string SwaggerUiDirectory = "swagger-ui";
        private const string SwaggerRequestPath = "";

        public static IApplicationBuilder UseOpenApiServer(this IApplicationBuilder app,
                                                           IReadOnlyCollection<OpenApiDocument> specs,
                                                           string contentRoot)
        {
            Directory.CreateDirectory(contentRoot);

            var options = app.ApplicationServices.GetService<IOptions<MockServerOptions>>();
            var serverUrl = options?.Value.MockServerHost;

            WriteSpecs(contentRoot, specs);
            
            AddSpecsDownload(app, contentRoot);
            AddSwaggerUI(app, contentRoot);
            AddSpecsDiscoveryEndpoint(app, specs, serverUrl);

            

            return app;
        }

        private static void AddSpecsDownload(IApplicationBuilder app, string contentRoot)
        {
            var dir = Path.Combine(contentRoot, SpecsOutputDirectory);
            Directory.CreateDirectory(dir);

            app.UseStaticFiles(new StaticFileOptions
                               {
                                       RequestPath = $"/{SpecsRequestPath}",
                                       FileProvider = new PhysicalFileProvider(dir)
                               });
        }

        private static void AddSwaggerUI(IApplicationBuilder app, string contentRoot)
        {
            var dir = Path.Combine(contentRoot, SwaggerUiDirectory);
            Directory.CreateDirectory(dir);

            var options = new SharedOptions
                          {
                                  FileProvider = new PhysicalFileProvider(dir),
                                  RequestPath = SwaggerRequestPath
                          };

            app.UseDefaultFiles(new DefaultFilesOptions(options));
            app.UseStaticFiles(new StaticFileOptions(options));
        }

        private static void AddSpecsDiscoveryEndpoint(IApplicationBuilder app,
                                                      IEnumerable<OpenApiDocument> specs,
                                                      string serverUrl)
        {
            var availableSpecs = specs.Select(ToNameUrlMap);
            var json = JsonConvert.SerializeObject(availableSpecs);
            app.MapWhen(IsDiscoveryRequest, x => x.Run(HandleRequest));

            bool IsDiscoveryRequest(HttpContext x)
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                return x.Request.Path.Equals($"/{SpecsRequestPath}", comparison) ||
                       x.Request.Path.Equals($"/{SpecsRequestPath}/", comparison);
            }

            Task HandleRequest(HttpContext ctx)
            {
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(json);
            }

            Dictionary<string, string> ToNameUrlMap(OpenApiDocument x) =>
                    new Dictionary<string, string>
                    {
                            ["name"] = $"{x.Info.Title}_{x.Info.GetMajorVersion()}",
                            ["url"] = GetSpecUrl(serverUrl, x)
                    };
        }

        private static void WriteSpecs(string contentRoot, IEnumerable<OpenApiDocument> specs)
        {
            var dir = Path.Combine(contentRoot, SpecsOutputDirectory);

            foreach (var spec in specs)
            {
                WriteOneSpec(spec);
            }

            void WriteOneSpec(OpenApiDocument spec)
            {
                var path = GetSpecFilePath(dir, spec);
                var content = OpenApiSerializer.Serialize(spec);
                
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var writer = File.CreateText(path))
                {
                    writer.Write(content);
                }
            }
        }

        

        private static string GetSpecFilePath(string dir, OpenApiDocument spec)
        {
            var ver = $"v{spec.Info.GetMajorVersion()}";
            var name = spec.Info.Title.Replace(" ", "").ToLowerInvariant();
            return Path.Combine(dir, name, ver, SpecFilename);
        }

        private static string GetSpecUrl(string serverAddress, OpenApiDocument spec)
        {
            var ver = $"v{spec.Info.GetMajorVersion()}";
            var name = spec.Info.Title.Replace(" ", "").ToLowerInvariant();
            return UrlHelper.Join(serverAddress, SpecsRequestPath, name, ver, SpecFilename);
        }
    }
}