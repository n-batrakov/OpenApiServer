using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

using Microsoft.Extensions.Primitives;

using HttpMethod = System.Net.Http.HttpMethod;

namespace ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers
{
    public class ProxyRequestHandler : IMockServerRequestHandler
    {
        private IHttpClientFactory ClientFactory { get; }

        public ProxyRequestHandler(IHttpClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
        }

        public Task<IMockServerResponseContext> HandleAsync(IMockServerRequestContext context)
        {
            if (context.Host == null)
            {
                throw new Exception("Unable to find host to proxy the request.");
            }

            return Proxy(context);
        }

        private async Task<IMockServerResponseContext> Proxy(IMockServerRequestContext ctx)
        {
            var client = ClientFactory.CreateClient();
            var request = CreateRequest(ctx);

            var response = await client.SendAsync(request).ConfigureAwait(false);

            return await CreateResponseAsync(response).ConfigureAwait(false);
        }

        private static HttpRequestMessage CreateRequest(IMockServerRequestContext ctx)
        {
            var targetRequest = new HttpRequestMessage
                                {
                                        RequestUri = GetUri(),
                                        Method = new HttpMethod(ctx.Method.ToString().ToUpperInvariant()),
                                        Content = GetContent()
                                };
            
            CopyHeaders(targetRequest);

            return targetRequest;

            Uri GetUri() => new Uri($"{ctx.Host}{ctx.PathAndQuery}");

            HttpContent GetContent()
            {
                return new StringContent(ctx.Body, Encoding.UTF8, ctx.ContentType);
            }

            void CopyHeaders(HttpRequestMessage x)
            {
                foreach (var (k, v) in ctx.Headers)
                {
                    x.Headers.TryAddWithoutValidation(k, v.ToArray());
                }
            }
        }

        private static Task<IMockServerResponseContext> CreateResponseAsync(HttpResponseMessage sourceResponse)
        {
            return sourceResponse.Content.ReadAsStringAsync().ContinueWith(CreateContext);

            IMockServerResponseContext CreateContext(Task<string> body) =>
                    new MockServerResponseContext
                    {
                            ContentType = sourceResponse.Content.Headers.ContentType.ToString(),
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