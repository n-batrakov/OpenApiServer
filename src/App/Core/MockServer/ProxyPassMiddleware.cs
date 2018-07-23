using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Validation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class ProxyPassMiddleware
    {
        private const string ProxyHeaderName = "X-Forwarded-Host";

        private RequestDelegate Next { get; }
        private IHttpClientFactory ClientFactory { get; }
        private IResponseValidator Validator { get; }

        public ProxyPassMiddleware(RequestDelegate next,
                                   IHttpClientFactory clientFactory,
                                   IResponseValidator validator)
        {
            Next = next;
            ClientFactory = clientFactory;
            Validator = validator;
        }

        public Task InvokeAsync(HttpContext ctx)
        {
            var hasProxyHeader = TryGetProxyHeader(ctx.Request, out var host);
            return hasProxyHeader ? Proxy(ctx, host) : Next(ctx);
        }

        private Task Proxy(HttpContext ctx, string host)
        {
            var client = ClientFactory.CreateClient();
            var request = CreateRequest(ctx.Request, host);
            var response = client.SendAsync(request);

            return response.ContinueWith(x => WriteResponse(x.Result, ctx.Response));
        }

        private static HttpRequestMessage CreateRequest(HttpRequest sourceRequest, string forwardHost)
        {
            var targetRequest = new HttpRequestMessage
                                {
                                        RequestUri = GetUri(),
                                        Method = new HttpMethod(sourceRequest.Method),
                                        Content = GetContent()
                                };
            CopyHeaders(targetRequest);

            return targetRequest;

            Uri GetUri() => new Uri($"{forwardHost}/{sourceRequest.GetEncodedPathAndQuery()}");

            HttpContent GetContent()
            {
                return new StreamContent(sourceRequest.Body);
            }

            void CopyHeaders(HttpRequestMessage x)
            {
                foreach (var (k, v) in sourceRequest.Headers)
                {
                    x.Headers.TryAddWithoutValidation(k, v.ToArray());
                }
            }
        }

        private static Task WriteResponse(HttpResponseMessage sourceResponse, HttpResponse targetResponse)
        {
            targetResponse.StatusCode = (int)sourceResponse.StatusCode;
            CopyHeaders();
            return CopyBody();

            void CopyHeaders()
            {
                // Setting {Transer-Encoding = chunked} on response results in invalid response.
                sourceResponse.Headers.Remove("Transfer-Encoding");

                foreach (var (k, v) in sourceResponse.Headers)
                {
                    targetResponse.Headers.Add(k, v.ToArray());
                }

                foreach (var (k, v) in sourceResponse.Content.Headers)
                {
                    targetResponse.Headers.Add(k, v.ToArray());
                }
            }

            Task CopyBody() =>
                    sourceResponse.Content.CopyToAsync(targetResponse.Body);
        }

        private static bool TryGetProxyHeader(HttpRequest request, out string result)
        {
            var hasProxyHeader = request.Headers.TryGetValue(ProxyHeaderName, out var header);
            result = header;
            return hasProxyHeader && header.Count > 0;
        }
    }
}