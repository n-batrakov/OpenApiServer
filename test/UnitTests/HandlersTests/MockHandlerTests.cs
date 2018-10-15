using System;
using System.Collections.Generic;
using System.Net;

using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Handlers.Defaults;
using OpenApiServer.Core.MockServer.MockDataProviders;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.HandlersTests
{
    public class MockHandlerTests
    {
        private static MockHandler Sut => new MockHandler(new TestMockDataProvider());

        [Fact]
        public void CanMockRequest()
        {
            var ctx = GetRouteContext(x => x.WithResponse(Schema.Object()));

            var expected = ResponseContext();

            var actual = Sut.HandleAsync(ctx).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanChooseResponse()
        {
            var ctx = GetRouteContext(x => x.WithResponse(Schema.Any(), contentType: "text/plain")
                                            .WithResponse(Schema.Object()));

            var expected = ResponseContext();

            var actual = Sut.HandleAsync(ctx).Result;

            DeepAssert.Equal(expected, actual);
        }

        private static RouteContext GetRouteContext(Action<RouteSpecBuilder> configure)
        {
            var builder = new RouteSpecBuilder();
            configure(builder);
            var spec = builder.Build();
            return RouteContextBuilder.FromUrl("/").WithSpec(spec).Build();
        }

        private static ResponseContext ResponseContext(string body = "test", HttpStatusCode statusCode = HttpStatusCode.OK) =>
                new ResponseContext
                {
                        Body = body,
                        BreakPipeline = false,
                        ContentType = "application/json",
                        Headers = new Dictionary<string, StringValues>(),
                        StatusCode = statusCode
                };

        private class TestMockDataProvider : IMockDataProvider
        {
            public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
            {
                writer.WriteRaw("test");
                return true;
            }
        }
    }
}