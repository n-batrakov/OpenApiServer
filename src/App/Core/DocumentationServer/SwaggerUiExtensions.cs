using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.FileProviders;

namespace OpenApiServer.Core.DocumentationServer
{
    public static class SwaggerUiExtensions
    {
        public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app)
        {
            var assm = typeof(Program).Assembly;
            var ns = $"{typeof(Program).Namespace}.Resources.SwaggerUI";
            var provider = new EmbeddedFileProvider(assm, ns);

            var filesOptions = new SharedOptions
                               {
                                       FileProvider = provider,
                                       RequestPath = ""
                               };

            return app.UseDefaultFiles(new DefaultFilesOptions(filesOptions))
                      .UseStaticFiles(new StaticFileOptions(filesOptions));
        }
    }
}