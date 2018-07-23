using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.DocumentationServer;
using ITExpert.OpenApi.Server.Core.MockingProxy;
using ITExpert.OpenApi.Server.Core.MockingProxy.Options;
using ITExpert.OpenApi.Server.Core.MockServer;
using ITExpert.OpenApi.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            var host = Configuration.GetValue<string>("host");
            services.AddMockServer(x => x.Host = host);

            var routesConfig = new MockingProxyRouteOptions
                               {
                                       Path = "*",
                                       Method = MockingProxyHttpMethod.Any,
                                       Latency = 300,
                                       Mock = true,
                                       Validate = MockingProxyValidationMode.All
                               };
            services.AddMockingProxy(x => x.Routes.Add("*", routesConfig));
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var contentRoot = Path.Combine(env.ContentRootPath, "wwwroot");

            var defaultDir = Path.Combine(contentRoot, "specs");
            var specsDir = Configuration.GetValue("specs", defaultDir);
            var specs = OpenApiDocumentsProvider.GetDocuments(specsDir).ToArray();

            app.UseMockServer(specs)
               .UseOpenApiServer(specs, contentRoot)
               .UseMockingProxy();
        }
    }
}
