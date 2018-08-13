using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Writers;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.ExampleProviders;
using OpenApiServer.Utils;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    public class MockRequestHandler : IRequestHandler
    {
        private IOpenApiExampleProvider ExampleProvider { get; }

        public MockRequestHandler(IOpenApiExampleProvider exampleProvider)
        {
            ExampleProvider = exampleProvider;
        }

        public Task<ResponseContext> HandleAsync(RequestContext context)
        {
            var responseSpec = ChooseResponse(context.Spec.Responses);
            if (responseSpec == null)
            {
                return Task.FromResult(RespondWithNothing(HttpStatusCode.NoContent));
            }

            var responseMock = MockResponse(responseSpec);
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

        private MockHttpResponse MockResponse(RequestContextResponse mediaType)
        {
            var body = OpenApiSerializer.Serialize(WriteBody);

            return new MockHttpResponse(body);

            void WriteBody(IOpenApiWriter writer)
            {
                var _ = TryWriteExample(writer, mediaType) || ExampleProvider.TryWriteValue(writer, mediaType.Schema);
            }
        }

        private static bool TryWriteExample(IOpenApiWriter writer, RequestContextResponse mediaType)
        {
            if (mediaType.Examples?.Count > 0)
            {
                var example = mediaType.Examples.First();
                writer.WriteRaw(example);
                return true;
            }

            return false;
        }

        private static ResponseContext RespondWithMock(MockHttpResponse mock, RequestContextResponse spec) =>
                new ResponseContext
                {
                        ContentType = spec.ContentType,
                        StatusCode = spec.StatusCodeParsed,
                        Headers = mock.Headers.ToDictionary(x => x.Key, x => new StringValues(x.Value)),
                        Body = mock.Body
                };

        private static ResponseContext RespondWithNothing(HttpStatusCode code) =>
                new ResponseContext {StatusCode = code};
    }
}