using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Generation.Internals;
using OpenApiServer.Core.MockServer.Options;
using OpenApiServer.Utils;

namespace OpenApiServer.Core.MockServer.Context
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

        public static string FormatUrl(this Microsoft.OpenApi.Models.OpenApiServer server)
        {
            //TODO: Format server url wtih variables
            return server.Url;
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

                                  ContentType = GetContentType(ctx.Request),
                                  Query = ctx.Request.Query,
                                  Headers = ctx.Request.Headers,
                                  Route = ctx.GetRouteData(),
                                  Body = GetBody(ctx.Request),
                          };
            return requestContext.WithRequest(callCtx, logger);
        }

        private static JToken GetBody(HttpRequest request)
        {
            return request.HasFormContentType ? ReadForm() : ReadBody();

            JToken ReadForm()
            {
                var obj = new JObject();
                foreach (var (key, value) in request.Form)
                {
                    var propertyValue = ParseRawValue(value);
                    obj.Add(key, propertyValue);
                }

                var fileProperties = request.Form.Files.ToDictionary(x => x.Name, GetFilePropertyValue);
                obj.AddRange(fileProperties, overwriteDuplicateKeys: false);

                return obj;
            }

            JToken ReadBody()
            {
                using (var reader = new StreamReader(request.Body))
                {
                    return ParseRawValue(reader.ReadToEnd());
                }
            }
        }

        private static JToken GetFilePropertyValue(IFormFile file)
        {
            using (var memorySteam = new MemoryStream())
            {
                file.CopyTo(memorySteam);
                var bytes = memorySteam.ToArray();
                var base64 = Convert.ToBase64String(bytes);
                return new JValue(base64);
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

        private static string GetContentType(HttpRequest request)
        {
            var hasContentType = request.Headers.TryGetValue("Content-Type", out var values);
            if (!hasContentType || values.Count == 0)
            {
                return null;
            }

            var contentType = values.First();
            return contentType.Split(';').First();
        }

        private static JToken ParseRawValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.StartsWith('{') || value.StartsWith('['))
            {
                return JToken.Parse(value);
            }

            if (bool.TryParse(value, out var boolean))
            {
                return new JValue(boolean);
            }

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            {
                return new JValue(num);
            }

            return new JValue(value);
        }
    }
}