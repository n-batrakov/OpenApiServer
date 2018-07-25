using System;

using ITExpert.OpenApi.Server.Core.MockServer.Generation;
using ITExpert.OpenApi.Server.Core.MockServer.Options;
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
            services.AddSingleton<IMockServerResponseValidator, NullResponseValidator>();
            services.AddSingleton<MockResponseGenerator>();

            services.AddSingleton<ProxyRequestHandler>();
            services.AddSingleton<MockRequestHandler>();
            services.AddSingleton<IMockServerRequestHandler, MockServerRequestHandler>();

            
            services.AddSingleton<IOpenApiOperationPathProvider, ConfigOperationPathProvider>();
            services.AddSingleton<MockServerRequestContextProvider>();

            return services;
        }

        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app, params OpenApiDocument[] specs)
        {
            var ctxProvider = app.ApplicationServices.GetRequiredService<MockServerRequestContextProvider>();
            var reqValidator = app.ApplicationServices.GetRequiredService<IMockServerRequestValidator>();
            var respValidator = app.ApplicationServices.GetRequiredService<IMockServerResponseValidator>();
            var handler = app.ApplicationServices.GetRequiredService<IMockServerRequestHandler>();
            var builder = new MockServerBuilder(ctxProvider, reqValidator, respValidator, handler);

            var routeBuilder = new RouteBuilder(app);
            var router = builder.BuildRouter(routeBuilder);

            app.UseRouter(router);

            return app;
        }
    }
}