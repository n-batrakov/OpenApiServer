using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Core.MockServer.Exceptions;

using Microsoft.Extensions.Primitives;

using HttpMethod = System.Net.Http.HttpMethod;

namespace ITExpert.OpenApi.Core.MockServer.RequestHandlers
{
    public class ProxyRequestHandler : IMockServerRequestHandler
    {
        private IHttpClientFactory ClientFactory { get; }

        public ProxyRequestHandler(IHttpClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
        }

        public Task<MockServerResponseContext> HandleAsync(RequestContext context)
        {
            if (context.Config.Host == default)
            {
                throw new MockServerConfigurationException("Unable to find host to proxy the request.");
            }

            return Proxy(context);
        }

        private async Task<MockServerResponseContext> Proxy(RequestContext ctx)
        {
            var client = ClientFactory.CreateClient();
            var request = CreateRequest(ctx);

            var response = await client.SendAsync(request).ConfigureAwait(false);

            return await CreateResponseAsync(response).ConfigureAwait(false);
        }

        private static HttpRequestMessage CreateRequest(RequestContext ctx)
        {
            var targetRequest = new HttpRequestMessage
                                {
                                        RequestUri = new Uri($"{ctx.Config.Host}{ctx.Request.PathAndQuery}"),
                                        Method = new HttpMethod(ctx.Request.Method.ToString().ToUpperInvariant()),
                                        Content = new StringContent(ctx.Request.Body, Encoding.UTF8, ctx.Request.ContentType)
                                };

            foreach (var (k, v) in ctx.Request.Headers)
            {
                targetRequest.Headers.TryAddWithoutValidation(k, v.ToArray());
            }

            return targetRequest;
        }

        private static Task<MockServerResponseContext> CreateResponseAsync(HttpResponseMessage sourceResponse)
        {
            return sourceResponse.Content.ReadAsStringAsync().ContinueWith(CreateContext);

            MockServerResponseContext CreateContext(Task<string> body) =>
                    new MockServerResponseContext
                    {
                            ContentType = sourceResponse.Content.Headers.ContentType?.ToString(),
                            StatusCode = sourceResponse.StatusCode,
                            Body = body.Result,
                            Headers = GetHeaders()
                    };

            IDictionary<string, StringValues> GetHeaders()
            {
                // Setting {Transer-Encoding = chunked} on response results in invalid response.
                var items = sourceResponse.Headers
                                          .Where(x => x.Key != "Transfer-Encoding")
                                          .Concat(sourceResponse.Content.Headers)
                                          .ToDictionary(x => x.Key, x => new StringValues(x.Value.ToArray()));
                return new Dictionary<string, StringValues>(items);
            }
        }
    }
}