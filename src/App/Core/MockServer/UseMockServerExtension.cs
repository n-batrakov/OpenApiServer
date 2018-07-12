using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public static class UseMockServerExtension
    {
        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app, params OpenApiDocument[] specs)
        {
            var routeBuilder = new MockRouteBuilder(app, specs);
            return app.UseRouter(routeBuilder.Build());
        }
    }
}