using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Context.Types.Spec;
using OpenApiServer.Core.MockServer.Exceptions;
using OpenApiServer.Utils;

using HttpMethod = System.Net.Http.HttpMethod;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("proxy", typeof(Options))]
    public class ProxyHandler : IRequestHandler
    {
        public class Options
        {
            public string Host { get; set; }
        }

        private static readonly Guid Id = Guid.NewGuid();
        private string ProxyInstanceId { get; }

        private const string ForwarderFromHeader = "X-Forwarded-From";
        private const string ForwardedHostHeader = "X-Forwarded-Host";

        private IHttpClientFactory ClientFactory { get; }
        private Options Config { get; }

        public ProxyHandler(IHttpClientFactory clientFactory, Options options)
        {
            ClientFactory = clientFactory;
            ProxyInstanceId = Id.ToString();
            Config = options;
        }

        public Task<ResponseContext> HandleAsync(RouteContext request)
        {
            var host = GetHostFromHeader(request.Request.Headers) ??
                       Config.Host ??
                       GetHostFromOperation(request.Spec);
            if (host == null)
            {
                return Task.FromResult<ResponseContext>(null);
            }

            var hasHeader = request.Request.Headers.TryGetValue(ForwarderFromHeader, out var mockServerHeader);
            if (hasHeader)
            {
                if (mockServerHeader == ProxyInstanceId)
                {
                    throw new MockServerConfigurationException(
                            "This MockServer instance is configured to proxy requests to itself, " +
                            "which will result in an infinite loop. Update the host via config or " +
                            "'X-Forwarded-Host' header");
                }
            }

            return Proxy(request, host);
        }

        private async Task<ResponseContext> Proxy(RouteContext ctx, string host)
        {
            var client = ClientFactory.CreateClient();
            var request = CreateRequest(ctx, host);

            var response = await client.SendAsync(request).ConfigureAwait(false);

            return await CreateResponseAsync(response).ConfigureAwait(false);
        }

        private HttpRequestMessage CreateRequest(RouteContext ctx, string host)
        {
            var bodyContent = ctx.Request.Body?.ToString() ?? string.Empty;
            var targetRequest = new HttpRequestMessage
            {
                RequestUri = new Uri($"{host}{ctx.Request.PathAndQuery}"),
                Method = new HttpMethod(ctx.Request.Method.ToString().ToUpperInvariant()),
                Content = new StringContent(bodyContent, Encoding.UTF8, ctx.Request.ContentType)
            };

            foreach (var (k, v) in ctx.Request.Headers)
            {
                // Skip Content-Length to avoid mismatch with actual content-length
                // which may change when request is recreated.
                if (k == "Content-Length") continue;

                targetRequest.Content.Headers.TryAddWithoutValidation(k, v.ToArray());
                targetRequest.Headers.TryAddWithoutValidation(k, v.ToArray());
            }

            targetRequest.Content.Headers.Add(ForwarderFromHeader, ProxyInstanceId);

            return targetRequest;
        }

        private static Task<ResponseContext> CreateResponseAsync(HttpResponseMessage sourceResponse)
        {
            var ctx = new ResponseContext
                      {
                              ContentType = sourceResponse.Content?.Headers.ContentType?.ToString(),
                              StatusCode = sourceResponse.StatusCode,
                              Headers = GetHeaders()
                      };

            return sourceResponse.Content == null
                           ? Task.FromResult(ctx)
                           : sourceResponse.Content.ReadAsStringAsync().ContinueWith(AssignBody);

            ResponseContext AssignBody(Task<string> body)
            {
                ctx.Body = body.Result;
                return ctx;
            }

            IDictionary<string, StringValues> GetHeaders()
            {
                // Setting {Transfer-Encoding = chunked} on response results in error.
                var items = sourceResponse.Headers.Where(x => x.Key != "Transfer-Encoding");

                if (sourceResponse.Content != null)
                {
                    items = items.Concat(sourceResponse.Content.Headers);
                }
                
                var headers = items.ToDictionary(x => x.Key, x => new StringValues(x.Value.ToArray()));
                return new Dictionary<string, StringValues>(headers);
            }
        }

        private static string GetHostFromHeader(IHeaderDictionary headers)
        {
            var hasProxyHeader = headers.TryGetValue(ForwardedHostHeader, out var header);
            if (hasProxyHeader)
            {
                return UrlHelper.GetHost(header.ToString());
            }
            else
            {
                return null;
            }
        }

        private static string GetHostFromOperation(RouteSpec spec)
        {
            var specUrl = spec?.Servers.FirstOrDefault();
            return specUrl == null ? null : UrlHelper.GetHost(specUrl);
        }
    }
}