using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockServerContext
    {
        public OpenApiOperation OperationSpec { get; }
        public MockServerOptionsRoute RouteOptions { get; }

    }

    public class MockServerContextProviderMiddleware
    {
        private RequestDelegate Next { get; }
        private IOptionsMonitor<MockServerOptions> Options { get; }

        public MockServerContextProviderMiddleware(RequestDelegate next,
                                                   IOptionsMonitor<MockServerOptions> options,
                                                   IEnumerable<OpenApiDocument> specs)
        {
            Next = next;
            Options = options;
        }

        public Task InvokeAsync(HttpContext ctx)
        {
            //TODO: Implement
            return Next(ctx);
        }
    }

    internal static class ContextProvider
    {
        public static readonly string Key = "MockServerContext";

        public static MockServerContext GetMockContext(this HttpContext ctx)
        {
            var hasContext = ctx.Items.TryGetValue(Key, out var value);
            if (hasContext && value is MockServerContext result)
            {
                return result;
            }

            throw new Exception("Unable to retrieve MockServer context. Make sure it's set before using.");
        }
    }
}