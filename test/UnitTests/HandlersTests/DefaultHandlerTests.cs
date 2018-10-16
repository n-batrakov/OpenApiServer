using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Handlers.Defaults;
using OpenApiServer.Core.MockServer.MockDataProviders;
using OpenApiServer.Core.MockServer.Validation;
using OpenApiServer.Core.MockServer.Validation.Types;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.HandlersTests
{
    public class DefaultHandlerTests
    {
        private static RouteContext RouteContext =>
                RouteContextBuilder
                        .FromUrl("/")
                        .WithHeaders(("X-Forwarded-Host", "http://test"))
                        .WithSpec(new RouteSpecBuilder()
                                  .WithResponse(Schema.Any())
                                  .Build())
                        .Build();

        [Fact]
        public void CanMock()
        {
            var sut = Sut(x => x.Mock = DefaultRequestHandler.MockMode.Replace);

            var expected = new ResponseContext
                           {
                                   Body = "{\"mock\": true}",
                                   ContentType = "application/json",
                                   StatusCode = HttpStatusCode.OK
                           };

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanProxy()
        {
            var sut = Sut(x => x.Proxy = "http://test");
            var expected = new ResponseContext
                           {
                                   Body = "{\"proxy\": true}",
                                   ContentType = "application/json",
                                   StatusCode = HttpStatusCode.OK,
                                   Headers = {{"Content-Type", "application/json"}}
                           };

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanProxyAndMock()
        {
            var sut = Sut(x =>
                          {
                              x.Proxy = "http://test";
                              x.Mock = DefaultRequestHandler.MockMode.Merge;
                          });
            var expected = new ResponseContext
                           {
                                   Body = JToken.Parse("{proxy: true, mock: true}").ToString(),
                                   ContentType = "application/json",
                                   StatusCode = HttpStatusCode.OK,
                                   Headers = {{"Content-Type", "application/json"}}
                           };

            var actual = sut.HandleAsync(RouteContext).Result;
            actual.Body = JToken.Parse(actual.Body).ToString();

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanValidateRequest()
        {
            var sut = Sut(x => x.Validate = DefaultRequestHandler.ValidationMode.Request, failRequest: true);

            var expectedBody = "{isSuccess: false, errors: [{ code: 'Request', description: 'Test', inner: [] }]}";
            var expected = new ResponseContext
                           {
                                   Body = JToken.Parse(expectedBody).ToString(),
                                   ContentType = "application/json",
                                   StatusCode = HttpStatusCode.BadRequest,
                                   BreakPipeline = true
                           };

            var actual = sut.HandleAsync(RouteContext).Result;
            actual.Body = JToken.Parse(actual.Body).ToString();

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanValidateResponse()
        {
            var sut = Sut(x =>
                          {
                              x.Validate = DefaultRequestHandler.ValidationMode.Response;
                              x.Mock = DefaultRequestHandler.MockMode.Replace;
                          }, failResponse: true);

            var expectedBody = "{isSuccess: false, errors: [{ code: 'Response', description: 'Test', inner: [] }]}";
            var expected = new ResponseContext
                           {
                                   Body = JToken.Parse(expectedBody).ToString(),
                                   ContentType = "application/json",
                                   StatusCode = HttpStatusCode.BadRequest,
                                   BreakPipeline = true
                           };

            var actual = sut.HandleAsync(RouteContext).Result;
            actual.Body = JToken.Parse(actual.Body).ToString();

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanSkipValidation()
        {
            var sut = Sut(x => x.Validate = DefaultRequestHandler.ValidationMode.None,
                          failRequest: true,
                          failResponse: true);

            var expected = new ResponseContext
                           {
                                   Body = "{\"proxy\": true}",
                                   ContentType = "application/json",
                                   StatusCode = HttpStatusCode.OK,
                                   Headers = { { "Content-Type", "application/json" } }
                           };

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanDelay()
        {
            var sut = Sut(x =>
                          {
                              x.Mock = DefaultRequestHandler.MockMode.Replace;
                              x.Delay = 10;
                          });
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var _ = sut.HandleAsync(RouteContext).Result;

            stopwatch.Stop();

            Assert.InRange(stopwatch.ElapsedMilliseconds, 10, 100);
        }



        private static DefaultRequestHandler Sut(Action<DefaultRequestHandler.Options> configure, bool failRequest = false, bool failResponse = false)
        {
            var options = new DefaultRequestHandler.Options();
            configure(options);

            var validator = new NullValidator(failRequest, failResponse);
            var dataProvider = new FakeMockDataProvider();
            var clientFactory = new FakeHttpClientFactory();
            return new DefaultRequestHandler(options, validator, validator, dataProvider, clientFactory);
        }

        private class NullValidator : IRequestValidator, IResponseValidator
        {
            private bool FailRequest { get; }
            private bool FailResponse { get; }

            public NullValidator(bool failRequest, bool failResponse)
            {
                FailRequest = failRequest;
                FailResponse = failResponse;
            }

            public HttpValidationStatus Validate(RouteContext context) =>
                    FailRequest
                            ? HttpValidationStatus.Error(new HttpValidationError("Request", "Test"))
                            : HttpValidationStatus.Success();

            public HttpValidationStatus Validate(ResponseContext response, RouteContext context) =>
                    FailResponse
                            ? HttpValidationStatus.Error(new HttpValidationError("Response", "Test"))
                            : HttpValidationStatus.Success();
        }

        private class FakeMockDataProvider : IMockDataProvider
        {
            public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
            {
                writer.WriteRaw("{\"mock\": true}");
                return true;
            }
        }

        private class FakeHttpClientFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name) => new HttpClient(new FakeHttpMessageHandler());

            private class FakeHttpMessageHandler : HttpMessageHandler
            {
                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                {
                    var contentString = "{\"proxy\": true}";
                    var bytes = Encoding.UTF8.GetBytes(contentString);
                    var content = new ReadOnlyMemoryContent(new ReadOnlyMemory<byte>(bytes));

                    var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    return Task.FromResult(response);
                }
            }
        }
    }
}