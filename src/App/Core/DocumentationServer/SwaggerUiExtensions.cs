using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.FileProviders;

namespace ITExpert.OpenApi.Core.DocumentationServer
{
    public static class SwaggerUiExtensions
    {
        public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app)
        {
            var assm = typeof(SwaggerUiExtensions).Assembly;
            var name = assm.GetName().Name;
            var provider = new EmbeddedFileProvider(assm, $"{name}.Resources.SwaggerUI");

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