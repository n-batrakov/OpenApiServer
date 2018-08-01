using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.DocumentationServer;
using ITExpert.OpenApi.Server.Core.MockServer;
using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.DocumentProviders;
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

        private string ContentRoot { get; }
        private string Host { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;

            ContentRoot = env.ContentRootPath;

            Host = Configuration[nameof(MockServerOptions.MockServerHost)];
        }

        // ReSharper disable once UnusedMember.Global
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMockServer(Configuration);

            var hasProvider = services.Any(x => x.ServiceType == typeof(IOpenApiDocumentProvider));
            if (!hasProvider)
            {
                AddDefaultDocumentsProvider(services);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app)
        {
            var specs = app.ApplicationServices
                           .GetRequiredService<IOpenApiDocumentProvider>()
                           .GetDocuments()
                           .ToArray();

            if (Host != null)
            {
                AddMockServerToSpecs(specs);
            }


            app.UseMockServer(specs)
               .UseOpenApiDocumentServer(specs, Host)
               .UseSwaggerUI();
        }

        private void AddDefaultDocumentsProvider(IServiceCollection services)
        {
            var defaultDir = Path.Combine(ContentRoot, "specs");
            var specsDir = Configuration.GetValue("specs", defaultDir);
            var provider = new DirectoryOpenApiDocumentsProvider(specsDir);
            services.AddSingleton<IOpenApiDocumentProvider>(provider);
        }

        private void AddMockServerToSpecs(IEnumerable<OpenApiDocument> specs)
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

                var mockServerUrl = UrlHelper.Join(Host, prefix);
                var mockServer = new OpenApiServer { Url = mockServerUrl };
                spec.Servers.Add(mockServer);
            }

            bool IsAbsoluteUrl(string url)
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                return url.StartsWith("http://", comparison) || url.StartsWith("https://", comparison);
            }
        }
    }
}

