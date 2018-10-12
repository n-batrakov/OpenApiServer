using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using OpenApiServer.Core.MockServer.Context;
using OpenApiServer.Core.MockServer.ExampleProviders;
using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Core.MockServer.Handlers.Internals;
using OpenApiServer.Core.MockServer.Options;
using OpenApiServer.Core.MockServer.Validation;

namespace OpenApiServer.Core.MockServer
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

            services.AddSingleton<IRequestValidator, RequestValidator>();
            services.AddSingleton<IResponseValidator, ResponseValidator>();

            services.AddSingleton<IOpenApiExampleProvider, OpenApiExampleProvider>();

            services.AddSingleton(x => new RequestHandlerActivator(x));
            services.AddSingleton(x => RequestHandlerProvider.FromAssemblies(x, typeof(Program).Assembly));

            services.AddSingleton<RouteContextProvider>();

            return services;
        }

        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app, params OpenApiDocument[] specs)
        {
            var builder = new MockServerBuilder(app, specs);
            var router = builder.Build();
            app.UseRouter(router);

            return app;
        }
    }
}