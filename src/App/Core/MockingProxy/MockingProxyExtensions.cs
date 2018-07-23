using System;

using ITExpert.OpenApi.Server.Core.MockingProxy.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ITExpert.OpenApi.Server.Core.MockingProxy
{
    public static class MockingProxyExtensions
    {
        public static IServiceCollection AddMockingProxy(this IServiceCollection services) =>
                AddMockingProxy(services, _ => { });

        public static IServiceCollection AddMockingProxy(this IServiceCollection services,
                                                         Action<MockingProxyOptions> configure) => 
                services.Configure(configure);

        public static IApplicationBuilder UseMockingProxy(this IApplicationBuilder app) =>
                app.UseMiddleware<MockingProxyMiddleware>();
    }
}