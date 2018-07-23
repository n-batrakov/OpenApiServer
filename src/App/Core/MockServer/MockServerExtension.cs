using System;

using ITExpert.OpenApi.Server.Core.MockServer.Generation;
using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Core.MockServer.Validation;

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
            services.AddRouting();
            services.AddHttpClient();
            services.AddOptions<MockServerOptions>().Configure(configure);
            services.AddSingleton<IMockServerRequestValidator, MockServerRequestValidator>();
            services.AddSingleton<IResponseValidator, NullResponseValidator>();
            services.AddSingleton<MockResponseGenerator>();

            return services;
        }

        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app, params OpenApiDocument[] specs)
        {
            app.UseMiddleware<MockServerContextProviderMiddleware>();
            app.UseMiddleware<ValidateRequestMiddleware>();
            app.UseMiddleware<ProxyPassMiddleware>();

            var routeBuilder = new MockRouteBuilder(app, specs);
            app.UseRouter(routeBuilder.Build());

            return app;
        }
    }
}