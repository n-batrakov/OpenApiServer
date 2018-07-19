using System;
using System.Collections.Generic;
using System.Globalization;
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
            var validationStatus = Validator.Validate(requestCtx, Operation);
            if (validationStatus.IsFaulty)
            {
                return RespondWithBadRequest(ctx.Response, validationStatus.Errors);
            }

            var responseSpec = ChooseResponse();
            var statusCode = int.Parse(responseSpec.Key, CultureInfo.InvariantCulture);

            var mediaType = GetMediaType(responseSpec.Value);
            if (mediaType == null)
            {
                return RespondWithNothing(ctx.Response, statusCode);
            }

            var responseMock = Generator.MockResponse(mediaType);
            return RespondWithMock(ctx.Response, responseMock, statusCode);
        }



        private KeyValuePair<string, OpenApiResponse> ChooseResponse()
        {
            if (Operation.Responses.Count == 0)
            {
                throw new Exception("Invalid OpenApi Specification. Responses should be defined");
            }

            var success = Operation
                          .Responses
                          .FirstOrDefault(x => x.Key.StartsWith("2", StringComparison.Ordinal));
            return string.IsNullOrEmpty(success.Key) ? Operation.Responses.First() : success;
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

        private static Task RespondWithBadRequest(HttpResponse response, IEnumerable<RequestValidationError> errors)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.ContentType = "application/json";
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
            using (var reader = new StreamReader(request.Body))
            {
                return reader.ReadToEnd();
            }
        }
    }
}