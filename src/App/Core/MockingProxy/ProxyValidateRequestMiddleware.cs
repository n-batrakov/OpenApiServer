using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace ITExpert.OpenApi.Server.Core.MockingProxy
{
    public class ProxyValidateRequestMiddleware
    {
        private RequestDelegate Next { get; }

        public ProxyValidateRequestMiddleware(RequestDelegate next)
        {
            Next = next;
        }

        public Task InvokeAsync(HttpContext ctx)
        {
            return ShouldValidate() ? Validate(ctx) : Next(ctx);
        }

        private bool ShouldValidate() => true;

        private Task Validate(HttpContext ctx)
        {
            var isValid = true;

            if (isValid)
            {
                return Next(ctx);
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return ctx.Response.WriteAsync("Invalid request").ContinueWith(_ => false);
            }
        }
    }
}