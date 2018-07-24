using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.DocumentationServer;
using ITExpert.OpenApi.Server.Core.MockServer;
using ITExpert.OpenApi.Utils;

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
        private OpenApiDocument[] Specs { get; }
        
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;

            ContentRoot = Path.Combine(env.ContentRootPath, "wwwroot");

            var defaultDir = Path.Combine(ContentRoot, "specs");
            var specsDir = Configuration.GetValue("specs", defaultDir);
            Specs = OpenApiDocumentsProvider.GetDocuments(specsDir).ToArray();
        }

        // ReSharper disable once UnusedMember.Global
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEnumerable<OpenApiDocument>>(Specs);
            services.AddMockServer(Configuration);
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMockServer(Specs)
               .UseOpenApiServer(Specs, ContentRoot);
        }
    }
}
