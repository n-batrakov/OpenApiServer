using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Core.DocumentationServer;
using ITExpert.OpenApi.Core.MockServer;
using ITExpert.OpenApi.Core.MockServer.Options;
using ITExpert.OpenApi.DocumentProviders;
using ITExpert.OpenApi.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        private string Host { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Host = Configuration[nameof(MockServerOptions.MockServerHost)];
        }

        // ReSharper disable once UnusedMember.Global
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMockServer(Configuration);
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

