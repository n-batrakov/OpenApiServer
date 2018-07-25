using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Generation;

using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockRequestHandler : IMockServerRequestHandler
    {
        private MockResponseGenerator Generator { get; }

        public MockRequestHandler(MockResponseGenerator generator)
        {
            Generator = generator;
        }

        public Task<IMockServerResponseContext> HandleAsync(IMockServerRequestContext context)
        {
            var operation = context.OperationSpec;
            var (statusCode, responseSpec) = ChooseResponse(operation);

            var (contentType, mediaType) = GetMediaType(responseSpec);
            if (mediaType == null)
            {
                return Task.FromResult(RespondWithNothing(statusCode));
            }

            var responseMock = Generator.MockResponse(mediaType);
            return Task.FromResult(RespondWithMock(responseMock, contentType, statusCode));
        }

        private static KeyValuePair<string, OpenApiResponse> ChooseResponse(OpenApiOperation operation)
        {
            if (operation.Responses.Count == 0)
            {
                throw new Exception("Invalid OpenApi Specification. Responses should be defined");
            }

            var success = operation
                          .Responses
                          .FirstOrDefault(x => x.Key.StartsWith("2", StringComparison.Ordinal));
            return string.IsNullOrEmpty(success.Key) ? operation.Responses.First() : success;
        }

        private static KeyValuePair<string, OpenApiMediaType> GetMediaType(OpenApiResponse response)
        {
            if (response.Content.Count == 0)
            {
                return default;
            }

            var result = response.Content.FirstOrDefault(x => x.Key == "*/*" || x.Key == "application/json");
            if (string.IsNullOrEmpty(result.Key))
            {
                throw new NotSupportedException("MockServer only supports 'application/json' or '*/*' for now.");
            }

            return result;
        }

        private static IMockServerResponseContext RespondWithMock(MockHttpResponse mock,
                                                                  string contentType,
                                                                  string statusCode)
        {
            return new MockServerResponseContext
                   {
                           ContentType = contentType,
                           StatusCode = Enum.Parse<HttpStatusCode>(statusCode, ignoreCase: true),
                           Headers = mock.Headers.ToDictionary(x => x.Key, x => new StringValues(x.Value)),
                           Body = mock.Body
                   };
        }

        private static IMockServerResponseContext RespondWithNothing(string statusCode)
        {
            return new MockServerResponseContext
                   {
                           StatusCode = Enum.Parse<HttpStatusCode>(statusCode, ignoreCase: true)
                   };
        }
    }
}