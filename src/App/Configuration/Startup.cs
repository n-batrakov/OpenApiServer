using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApiServer.Utils;

using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.Configuration
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var defaultDir = Path.Combine(env.ContentRootPath, "Specs");
            var specsDir = Configuration.GetValue("specs", defaultDir);
            var specs = OpenApiDocumentsProvider.GetDocuments(specsDir).ToArray();
            WriteSpecs("wwwroot", specs);

            //app.UseMockServer(specs);

            app.UseFileServer(new FileServerOptions
                              {
                                      EnableDirectoryBrowsing = true,
                                      DirectoryBrowserOptions = { RequestPath = ""}
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
                var path = GetSpecFilePath(dir, spec.Info.Version, spec.Info.Title);
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
            var name = apiTitle.Replace(" ", "");
            return Path.Combine(dir, name, apiVersion, "swagger.json");
        }
    }
}
