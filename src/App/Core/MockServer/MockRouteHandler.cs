using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockRouteHandler
    {
        private OpenApiOperation Operation { get; }

        private RequestValidator Validator { get; }
        private MockResponseGenerator Generator { get; }

        public MockRouteHandler(OpenApiOperation operation,
                                RequestValidator validator,
                                MockResponseGenerator generator)
        {
            Operation = operation;
            Validator = validator;
            Generator = generator;
        }

        public Task InvokeAsync(HttpContext ctx)
        {
            var requestCtx = GetRequestContext(ctx.Request);
            var status = Validator.Validate(requestCtx, Operation);
            if (status.IsFaulty)
            {
                return RespondWithErrors(ctx.Response, status.Errors);
            }

            var responseSpec = ChooseResponse();
            var mediaType = GetMediaType(responseSpec);
            var responseMock = Generator.MockResponse(responseSpec, mediaType);

            return RespondWithMock(ctx.Response, responseMock);
        }

        private OpenApiMediaType GetMediaType(OpenApiResponse response)
        {
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

        private OpenApiResponse ChooseResponse()
        {
            if (Operation.Responses.Count == 0)
            {
                return null;
            }

            if (Operation.Responses.Count == 1)
            {
                return Operation.Responses.First().Value;
            }

            var firstSuccess = Operation
                               .Responses
                               .FirstOrDefault(x => x.Key.StartsWith("2", StringComparison.Ordinal))
                               .Value;
            if (firstSuccess != null)
            {
                return firstSuccess;
            }

            return Operation.Responses.First().Value;
        }

        private static Task RespondWithMock(HttpResponse response, MockHttpResponse mock)
        {
            response.StatusCode = mock.StatusCode;
            foreach (var header in mock.Headers)
            {
                response.Headers[header.Key] = header.Value;
            }

            response.ContentType = "application/json";

            return response.WriteAsync(mock.Body, Encoding.UTF8);
        }

        private static Task RespondWithErrors(HttpResponse response, IEnumerable<RequestValidationError> errors)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            var json = JsonConvert.SerializeObject(errors);
            return response.WriteAsync(json, Encoding.UTF8);
        }

        private static HttpRequestValidationContext GetRequestContext(HttpRequest request) =>
                new HttpRequestValidationContext
                {
                        Route = request.Path,
                        Headers = request.Headers,
                        Query = request.Query,
                        Form = ReadForm(request),
                        Body = ReadBody(request),
                        ContentType = request.ContentType
                };

        private static IFormCollection ReadForm(HttpRequest request)
        {
            return request.HasFormContentType ? request.Form : null;
        }

        private static string ReadBody(HttpRequest request)
        {
            // Note: reading the body without rewind makes it empty
            // for any subsequent reads.
            // Use `request.EnableRewind()` or similar.

            using (var reader = new StreamReader(request.Body))
            {
                return reader.ReadToEnd();
            }
        }
    }
}