using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Generation;

using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockRouteHandler
    {
        private MockResponseGenerator Generator { get; }

        public MockRouteHandler(MockResponseGenerator generator)
        {
            Generator = generator;
        }

        public Task InvokeAsync(HttpContext ctx)
        {
            var operation = ctx.GetMockContext().OperationSpec;
            var responseSpec = ChooseResponse(operation);
            var statusCode = int.Parse(responseSpec.Key, CultureInfo.InvariantCulture);

            var mediaType = GetMediaType(responseSpec.Value);
            if (mediaType == null)
            {
                return RespondWithNothing(ctx.Response, statusCode);
            }

            var responseMock = Generator.MockResponse(mediaType);
            return RespondWithMock(ctx.Response, responseMock, statusCode);
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

        private static OpenApiMediaType GetMediaType(OpenApiResponse response)
        {
            if (response.Content.Count == 0)
            {
                return null;
            }

            var hasJson = response.Content.TryGetValue("application/json", out var result);
            if (hasJson)
            {
                return result;
            }

            var hasAny = response.Content.TryGetValue("*/*", out result);
            if (hasAny)
            {
                return result;
            }

            throw new NotSupportedException("MockServer only supports 'application/json' or '*/*' for now.");
        }

        private static Task RespondWithMock(HttpResponse response, MockHttpResponse mock, int statusCode)
        {
            response.StatusCode = statusCode;

            foreach (var header in mock.Headers)
            {
                response.Headers[header.Key] = header.Value;
            }

            response.ContentType = "application/json";

            return response.WriteAsync(mock.Body, Encoding.UTF8);
        }

        private static Task RespondWithNothing(HttpResponse response, int statusCode)
        {
            response.StatusCode = statusCode;
            return Task.CompletedTask;
        }
    }
}