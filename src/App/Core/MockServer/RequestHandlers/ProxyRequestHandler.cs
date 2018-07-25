using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

using Microsoft.Extensions.Primitives;

namespace ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers
{
    public class ProxyRequestHandler : IMockServerRequestHandler
    {
        private const string ProxyHeaderName = "X-Forwarded-Host";

        private IHttpClientFactory ClientFactory { get; }

        public ProxyRequestHandler(IHttpClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
        }

        public Task<IMockServerResponseContext> HandleAsync(IMockServerRequestContext context)
        {
            var foundHost = TryGetProxyHeader(context, out var host);
            if (!foundHost)
            {
                throw new Exception("Unable to find host to proxy the request.");
            }

            return Proxy(context, host);
        }

        private async Task<IMockServerResponseContext> Proxy(IMockServerRequestContext ctx, string host)
        {
            var client = ClientFactory.CreateClient();
            var request = CreateRequest(ctx, host);
            var response = await client.SendAsync(request).ConfigureAwait(false);

            return await CreateResponseAsync(response).ConfigureAwait(false);
        }

        private static HttpRequestMessage CreateRequest(IMockServerRequestContext ctx, string forwardHost)
        {
            var targetRequest = new HttpRequestMessage
                                {
                                        RequestUri = GetUri(),
                                        Method = new HttpMethod(ctx.Method.ToString()),
                                        Content = GetContent()
                                };
            CopyHeaders(targetRequest);

            return targetRequest;

            Uri GetUri() => new Uri($"{forwardHost}/{ctx.PathAndQuery}");

            HttpContent GetContent()
            {
                return new StringContent(ctx.Body);
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

        private static bool TryGetProxyHeader(IMockServerRequestContext context, out string result)
        {
            var hasProxyHeader = context.Headers.TryGetValue(ProxyHeaderName, out var header);
            result = header;
            return hasProxyHeader && header.Count > 0;
        }
    }
}