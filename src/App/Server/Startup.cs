using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using OpenApiServer.Core.DocumentationServer;
using OpenApiServer.Core.MockServer;
using OpenApiServer.Core.MockServer.Options;
using OpenApiServer.DocumentProviders;
using OpenApiServer.Utils;

namespace OpenApiServer.Server
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

                var prefix = specServer == null
                                     ? UrlHelper.GetDefaultPathPrefix(spec.Info.GetServiceName())
                                     : UrlHelper.GetPathPrefix(specServer);

                var mockServerUrl = UrlHelper.Join(Host, prefix);
                var mockServer = new Microsoft.OpenApi.Models.OpenApiServer { Url = mockServerUrl };
                spec.Servers.Add(mockServer);
            }
        }
    }
}

