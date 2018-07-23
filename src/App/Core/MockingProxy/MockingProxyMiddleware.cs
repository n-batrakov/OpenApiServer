using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockingProxy.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ITExpert.OpenApi.Server.Core.MockingProxy
{
    public class MockingProxyMiddleware
    {
        private const string ProxyHeaderName = "X-Forwarded-Host";

        private IOptionsSnapshot<MockingProxyRouteOptions> Options { get; }
        private RequestDelegate Next { get; }

        public MockingProxyMiddleware(RequestDelegate next, IOptionsSnapshot<MockingProxyRouteOptions> options)
        {
            Next = next;
            Options = options;
        }

        public Task InvokeAsync(HttpContext ctx)
        {
            var hasProxyHeader = TryGetProxyHeader(ctx.Request, out var host);
            if (!hasProxyHeader)
            {
                return Next(ctx);
            }

            return Next(ctx);
        }

        private static bool TryGetProxyHeader(HttpRequest request, out string result)
        {
            var hasProxyHeader = request.Headers.TryGetValue(ProxyHeaderName, out var header);
            result = header;
            return hasProxyHeader && header.Count > 0;
        }
    }
}