using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Kestrel = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Exceptions;
using OpenApiServer.Core.MockServer.Handlers.Defaults;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.HandlersTests
{
    public class ProxyHandlerTests
    {
        [Fact]
        public void CanProxyRequest()
        {
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            var expected = new ResponseContext
                           {
                                   StatusCode = HttpStatusCode.OK,
                                   Body = "",
                                   ContentType = "application/json; charset=utf-8",
                                   Headers = {{"Content-Type", "application/json; charset=utf-8"}}
                           };

            var actual = Sut().HandleAsync(ctx)?.Result.RemoveSystemHeader();

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanProxyWithBody()
        {
            var ctx = RouteContextBuilder
                      .FromUrl("/", Kestrel.HttpMethod.Post)
                      .WithBody("\"Test\"")
                      .Build();

            var expected = new ResponseContext
                           {
                                   StatusCode = HttpStatusCode.OK,
                                   Body = "Test",
                                   ContentType = "application/json; charset=utf-8",
                                   Headers = {{"Content-Type", "application/json; charset=utf-8"}}
                           };

            var actual = Sut().HandleAsync(ctx).Result?.RemoveSystemHeader();

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanTakeTargetAddressFromHeader()
        {
            var ctx = RouteContextBuilder
                      .FromUrl("/")
                      .WithHeaders(("X-Forwarded-Host", "http://test"))
                      .Build();

            var expected = new ResponseContext
                           {
                                   StatusCode = HttpStatusCode.OK,
                                   Body = "",
                                   ContentType = "application/json; charset=utf-8",
                                   Headers =
                                   {
                                           {"Content-Type", "application/json; charset=utf-8"},
                                           {"X-Forwarded-Host", "http://test"}
                                   }
                           };

            var actual = Sut(null).HandleAsync(ctx).Result?.RemoveSystemHeader();

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanTakeTargetAddressFromSpec()
        {
            var spec = new RouteSpecBuilder().WithServer("http://test").Build();
            var ctx = RouteContextBuilder.FromUrl("/").WithSpec(spec).Build();

            var expected = new ResponseContext
                           {
                                   StatusCode = HttpStatusCode.OK,
                                   Body = "",
                                   ContentType = "application/json; charset=utf-8",
                                   Headers = {{"Content-Type", "application/json; charset=utf-8"}}
                           };

            var actual = Sut(null).HandleAsync(ctx).Result?.RemoveSystemHeader();

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnNullWhenTargetAddressNotFound()
        {
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            var actual = Sut(null).HandleAsync(ctx).Result;

            Assert.Null(actual);
        }

        [Fact]
        public async Task ThrowOnRecursion()
        {
            // When proxy is configured to proxy to self,
            // first it receives request from client.
            // Then it makes same request to self.
            // To simulate this behaviour, let's make use of EchoHttpHandler.
            // Calling proxy second time with response from the first time
            // Should be equivalent to calling self.

            var sut = Sut();

            var clientRequest = RouteContextBuilder.FromUrl("/").Build();
            var proxyFirstResponse = sut.HandleAsync(clientRequest).Result;

            var proxySelfRequest = RouteContextBuilder
                                   .FromUrl("/")
                                   .WithHeaders(proxyFirstResponse.Headers)
                                   .Build();

            await Assert.ThrowsAsync<MockServerConfigurationException>(() => sut.HandleAsync(proxySelfRequest));
        }


        private static ProxyHandler Sut(string host = "http://test")
        {
            var clientFactory = new HttpClientFactory(new EchoHttpHandler());
            return new ProxyHandler(clientFactory, new ProxyHandler.Options { Host = host });
        }

        private class HttpClientFactory : IHttpClientFactory
        {
            private HttpMessageHandler Handler { get; }

            public HttpClientFactory(HttpMessageHandler handler)
            {
                Handler = handler;
            }

            public HttpClient CreateClient(string name) => new HttpClient(Handler);

            
        }

        private class EchoHttpHandler : HttpMessageHandler
        {
            private string Url { get; }
            private HttpMethod Method { get; }

            public EchoHttpHandler() : this(null, null)
            {
            }

            private EchoHttpHandler(string url, HttpMethod method)
            {
                Url = url;
                Method = method;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (Url != null && !string.Equals(Url, request.RequestUri.LocalPath))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }

                if (Method != null && Method != request.Method)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }

                var response = new HttpResponseMessage
                               {
                                       Content = request.Content,
                                       StatusCode = HttpStatusCode.OK,
                                       RequestMessage = request
                               };

                foreach (var (key, values) in request.Headers)
                {
                    var value = values.ToArray();
                    request.Headers.Add(key, value);
                }

                return Task.FromResult(response);
            }
        }
    }

    public static class ResponseContextExtensions
    {
        /// <summary>
        /// Remove X-Forwarded-From system header.
        /// It's value is random guid, which is inconvenient for Asserts.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static ResponseContext RemoveSystemHeader(this ResponseContext ctx)
        {
            ctx.Headers.Remove("X-Forwarded-From");
            return ctx;
        }
    }
}