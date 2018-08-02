using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Utils;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Server.Core.MockServer.Context
{
    internal static class RequestContextExtensions
    {
        public static RouteId GetRouteId(this HttpContext ctx)
        {
            var routeData = ctx.GetRouteData();
            var route = routeData.Routers.OfType<Route>().FirstOrDefault();
            if (route == null)
            {
                throw new Exception($"Unable to find route for {ctx.Request.Path} ({ctx.Request.Method})");
            }

            var template = route.RouteTemplate;
            var verb = ctx.Request.Method.ToLowerInvariant();
            return new RouteId(template, verb);
        }

        public static RequestContextBody GetBodySpec(this RequestContext context)
        {
            if (context.Request == null)
            {
                throw new Exception("Unable to get body without RequestContext.Request");
            }

            var contentType = context.Request.ContentType;
            return context.Spec.Bodies.FirstOrDefault(x => x.ContentType == contentType || x.ContentType == "*/*");
        }

        public static bool IsMatch(this MockServerRouteOptions config, RouteId id)
        {
            var regexp = new Regex(config.Path);
            return regexp.IsMatch(id.Path) && IsMethodsMatch(id.Verb, config.Method);
        }

        private static bool IsMethodsMatch(HttpMethod specMethod, MockServerOptionsHttpMethod configMethod)
        {
            switch (configMethod)
            {
                case MockServerOptionsHttpMethod.Get:
                    return specMethod == HttpMethod.Get;
                case MockServerOptionsHttpMethod.Put:
                    return specMethod == HttpMethod.Put;
                case MockServerOptionsHttpMethod.Delete:
                    return specMethod == HttpMethod.Delete;
                case MockServerOptionsHttpMethod.Post:
                    return specMethod == HttpMethod.Post;
                case MockServerOptionsHttpMethod.Head:
                    return specMethod == HttpMethod.Head;
                case MockServerOptionsHttpMethod.Trace:
                    return specMethod == HttpMethod.Trace;
                case MockServerOptionsHttpMethod.Patch:
                    return specMethod == HttpMethod.Patch;
                case MockServerOptionsHttpMethod.Connect:
                    return specMethod == HttpMethod.Connect;
                case MockServerOptionsHttpMethod.Options:
                    return specMethod == HttpMethod.Options;
                case MockServerOptionsHttpMethod.Any:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(configMethod), configMethod, null);
            }
        }



        public static RequestContext WithRequest(this RequestContext requestContext, HttpContext ctx, ILogger logger = null)
        {
            var callCtx = new RequestContextCall
                          {
                                  PathAndQuery = ctx.Request.GetEncodedPathAndQuery(),
                                  Method = Enum.Parse<HttpMethod>(ctx.Request.Method, ignoreCase: true),
                                  Host = GetHostFromHeader(ctx) ??
                                         requestContext.Config.Host ??
                                         GetHostFromOperation(requestContext.Spec),

                                  ContentType = ctx.Request.ContentType,
                                  Query = ctx.Request.Query,
                                  Headers = ctx.Request.Headers,
                                  Route = ctx.GetRouteData(),
                                  Body = GetBody(ctx.Request),
            };
            return requestContext.WithRequest(callCtx, logger);
        }

        private static string GetBody(HttpRequest request)
        {
            return request.HasFormContentType ? ReadForm() : ReadBody();

            string ReadForm()
            {
                var dict = request.Form.ToDictionary(x => x.Key, x => JToken.Parse(x.Value));
                return JsonConvert.SerializeObject(dict);
            }

            string ReadBody()
            {
                using (var reader = new StreamReader(request.Body))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static string GetHostFromHeader(HttpContext ctx)
        {
            const string proxyHeaderName = "X-Forwarded-Host";
            var hasProxyHeader = ctx.Request.Headers.TryGetValue(proxyHeaderName, out var header);
            if (hasProxyHeader)
            {
                return UrlHelper.GetHost(header.ToString());
            }
            else
            {
                return null;
            }
        }

        private static string GetHostFromOperation(RequestContextSpec spec)
        {
            var specUrl = spec.Servers.FirstOrDefault();
            return specUrl == null ? null : UrlHelper.GetHost(specUrl);
        }

        public static string FormatUrl(this OpenApiServer server)
        {
            //TODO: Format server url wtih variables
            return server.Url;
        }
    }
}