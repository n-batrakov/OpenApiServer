using System;

using ITExpert.OpenApi.Server.Core.MockServer.Context;
using ITExpert.OpenApi.Server.Core.MockServer.Generation;
using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers;
using ITExpert.OpenApi.Server.Core.MockServer.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public static class MockServerExtension
    {
        public static IServiceCollection AddMockServer(this IServiceCollection services,
                                                       IConfiguration config)
        {
            services.Configure<MockServerOptions>(config);
            return AddMockServer(services);
        }

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
            services.AddSingleton<IMockServerResponseValidator, MockServerResponseValidator>();
            services.AddSingleton<MockResponseGenerator>();

            services.AddSingleton<ProxyRequestHandler>();
            services.AddSingleton<MockRequestHandler>();
            services.AddSingleton<IMockServerRequestHandler, MockServerRequestHandler>();


            services.AddSingleton<ContextProvider>();

            return services;
        }

        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app, params OpenApiDocument[] specs)
        {
            var builder = ActivatorUtilities.CreateInstance<MockServerBuilder>(app.ApplicationServices);
            var routeBuilder = new RouteBuilder(app);
            var router = builder.MapMockServerRoutes(routeBuilder);

            app.UseRouter(router.Build());

            return app;
        }
    }
}