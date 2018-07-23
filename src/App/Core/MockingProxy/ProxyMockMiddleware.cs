using System.Net.Http;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockingProxy.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ITExpert.OpenApi.Server.Core.MockingProxy
{
    public class ProxyMockMiddleware
    {
        private IOptions<MockingProxyRouteOptions> Options { get; }

        private RequestDelegate Next { get; }

        public ProxyMockMiddleware(RequestDelegate next,

                                      //IOptionsSnapshot<MockingProxyRouteOptions> options,
                                      IHttpClientFactory clientFactory)
        {
            Next = next;

            //Options = options;
        }

        // ReSharper disable once UnusedMember.Global
        public Task InvokeAsync(HttpContext ctx)
        {
            return ShouldMock() ? MockRequestAsync(ctx) : Next(ctx);
        }

        private bool ShouldMock() => false;

        private Task MockRequestAsync(HttpContext ctx)
        {
            return ctx.Response.WriteAsync("Mock").ContinueWith(x => Next(ctx));
        }
    }
}