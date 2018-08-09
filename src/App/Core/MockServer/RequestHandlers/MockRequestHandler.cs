using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Generation;
using OpenApiServer.Core.MockServer.Generation.Types;

namespace OpenApiServer.Core.MockServer.RequestHandlers
{
    public class MockRequestHandler : IMockServerRequestHandler
    {
        private MockResponseGenerator Generator { get; }

        public MockRequestHandler(MockResponseGenerator generator)
        {
            Generator = generator;
        }

        public Task<MockServerResponseContext> HandleAsync(RequestContext context)
        {
            var responseSpec = ChooseResponse(context.Spec.Responses);
            if (responseSpec == null)
            {
                return Task.FromResult(RespondWithNothing(HttpStatusCode.NoContent));
            }

            var responseMock = Generator.MockResponse(responseSpec);
            return Task.FromResult(RespondWithMock(responseMock, responseSpec));
        }

        private static RequestContextResponse ChooseResponse(IEnumerable<RequestContextResponse> responseSpec)
        {
            var filterMediaType =
                    responseSpec.Where(x => x.ContentType == "*/*" || x.ContentType == "application/json").ToArray();
            if (filterMediaType.Length == 0)
            {
                throw new NotSupportedException("MockServer only supports 'application/json' or '*/*' for now.");
            }

            var comparison = StringComparison.OrdinalIgnoreCase;
            var successResponse = filterMediaType.FirstOrDefault(x => x.StatusCode.StartsWith("2", comparison) ||
                                                                      x.StatusCode.Equals("default", comparison));

            return successResponse ?? filterMediaType.FirstOrDefault();
        }

        private static MockServerResponseContext RespondWithMock(MockHttpResponse mock, RequestContextResponse spec) =>
                new MockServerResponseContext
                {
                        ContentType = spec.ContentType,
                        StatusCode = spec.StatusCodeParsed,
                        Headers = mock.Headers.ToDictionary(x => x.Key, x => new StringValues(x.Value)),
                        Body = mock.Body
                };

        private static MockServerResponseContext RespondWithNothing(HttpStatusCode code) =>
                new MockServerResponseContext {StatusCode = code};
    }
}