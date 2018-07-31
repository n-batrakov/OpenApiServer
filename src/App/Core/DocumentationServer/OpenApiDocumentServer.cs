using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;

namespace ITExpert.OpenApi.Server.Core.DocumentationServer
{
    public static class OpenApiDocumentServer
    {
        public static IServiceCollection AddOpenApiServer(this IServiceCollection services,
                                                          IConfiguration config,
                                                          string contentRoot = default)
        {
            services.Configure<OpenApiDocumentServerOptions>(config);
            services.PostConfigure<OpenApiDocumentServerOptions>(
                    x =>
                    {
                        if (string.IsNullOrEmpty(contentRoot))
                        {
                            return;
                        }

                        x.SwaggerUi = x.SwaggerUi ?? Path.Join(contentRoot, "swagger-ui");
                        x.SpecsDirectory = x.SpecsDirectory ?? Path.Join(contentRoot, "out");
                    });

            return services;
        }

        public static IApplicationBuilder UseOpenApiServer(this IApplicationBuilder app,
                                                           IReadOnlyCollection<OpenApiDocument> specs)
        {
            var options = app.ApplicationServices.GetService<IOptions<OpenApiDocumentServerOptions>>().Value;

            if (!options.SkipWrite)
            {
                WriteSpecs(options, specs);
            }
            
            UseSpecsDownload(app, options);
            UseSwaggerUI(app, options);
            UseSpecsDiscoveryEndpoint(app, options, specs);

            return app;
        }

        private static void UseSpecsDownload(IApplicationBuilder app, OpenApiDocumentServerOptions options)
        {
            Directory.CreateDirectory(options.SpecsDirectory);

            app.UseStaticFiles(new StaticFileOptions
                               {
                                       RequestPath = $"/{options.SpecsUrl}",
                                       FileProvider = new PhysicalFileProvider(options.SpecsDirectory)
                               });
        }

        private static void UseSwaggerUI(IApplicationBuilder app, OpenApiDocumentServerOptions options)
        {
            Directory.CreateDirectory(options.SwaggerUi);

            var filesOptions = new SharedOptions
                          {
                                  FileProvider = new PhysicalFileProvider(options.SwaggerUi),
                                  RequestPath = options.SwaggerUrl
                          };

            app.UseDefaultFiles(new DefaultFilesOptions(filesOptions));
            app.UseStaticFiles(new StaticFileOptions(filesOptions));
        }

        private static void UseSpecsDiscoveryEndpoint(IApplicationBuilder app,
                                                      OpenApiDocumentServerOptions options,
                                                      IEnumerable<OpenApiDocument> specs)
        {
            var availableSpecs = specs.Select(ToNameUrlMap);
            var json = JsonConvert.SerializeObject(availableSpecs);
            app.MapWhen(IsDiscoveryRequest, x => x.Run(HandleRequest));

            bool IsDiscoveryRequest(HttpContext x)
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                return x.Request.Path.Equals($"/{options.SpecsUrl}", comparison) ||
                       x.Request.Path.Equals($"/{options.SpecsUrl}/", comparison);
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
                            ["url"] = GetSpecUrl(options.MockServerHost, x)
                    };

            string GetSpecUrl(string serverAddress, OpenApiDocument spec)
            {
                var ver = $"v{spec.Info.GetMajorVersion()}";
                var name = spec.Info.Title.Replace(" ", "").ToLowerInvariant();
                return UrlHelper.Join(serverAddress, options.SpecsUrl, name, ver, options.SpecFilename);
            }
        }

        private static void WriteSpecs(OpenApiDocumentServerOptions options, IEnumerable<OpenApiDocument> specs)
        {
            foreach (var spec in specs)
            {
                WriteOneSpec(spec);
            }

            void WriteOneSpec(OpenApiDocument spec)
            {
                var path = GetSpecFilePath(spec);
                var content = OpenApiSerializer.Serialize(spec);
                
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var writer = File.CreateText(path))
                {
                    writer.Write(content);
                }
            }

            string GetSpecFilePath(OpenApiDocument spec)
            {
                var ver = $"v{spec.Info.GetMajorVersion()}";
                var name = spec.Info.Title.Replace(" ", "").ToLowerInvariant();
                return Path.Combine(options.SpecsDirectory, name, ver, options.SpecFilename);
            }
        }
    }
}