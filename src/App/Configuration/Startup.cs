using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.DocumentationServer;
using ITExpert.OpenApi.Server.Core.MockServer;
using ITExpert.OpenApi.Server.Core.MockServer.Context;
using ITExpert.OpenApi.Server.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Configuration
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ReSharper disable once UnusedMember.Global
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMockServer(Configuration);
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var contentRoot = Path.Combine(env.ContentRootPath, "wwwroot");

            var defaultDir = Path.Combine(contentRoot, "specs");
            var specsDir = Configuration.GetValue("specs", defaultDir);
            var specs = OpenApiDocumentsProvider.GetDocuments(specsDir).ToArray();

            var mockServerHost = Configuration["mockServerHost"];
            if (mockServerHost != null)
            {
                AddMockServer(specs, mockServerHost);
            }
            

            app.UseMockServer(specs)
               .UseOpenApiServer(specs, contentRoot);
        }

        private static void AddMockServer(IEnumerable<OpenApiDocument> specs, string mockServerHost)
        {
            foreach (var spec in specs)
            {
                var specServer = spec.Servers.FirstOrDefault()?.Url;

                if (specServer != null && !IsAbsoluteUrl(specServer))
                {
                    continue;
                }

                var prefix = specServer == null
                                     ? UrlHelper.GetDefaultPathPrefix(spec.Info.GetServiceName())
                                     : UrlHelper.GetPathPrefix(specServer);

                var mockServerUrl = UrlHelper.Join(mockServerHost, prefix);
                var mockServer = new OpenApiServer { Url = mockServerUrl };
                spec.Servers.Add(mockServer);
            }
        }

        private static bool IsAbsoluteUrl(string url)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;
            return url.StartsWith("http://", comparison) || url.StartsWith("https://", comparison);
        }
    }
}
