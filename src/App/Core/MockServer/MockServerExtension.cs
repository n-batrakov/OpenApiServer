using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public static class MockServerExtension
    {
        public static IServiceCollection AddMockServer(this IServiceCollection services)
        {
            return AddMockServer(services, _ => { });
        }

        public static IServiceCollection AddMockServer(this IServiceCollection services,
                                                       Action<MockServerOptions> configure)
        {
            services.Configure(configure);
            services.AddRouting();
            return services;
        }

        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app, params OpenApiDocument[] specs)
        {
            var routeBuilder = new MockRouteBuilder(app, specs);
            return app.UseRouter(routeBuilder.Build());
        }
    }
}