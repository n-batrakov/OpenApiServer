using System;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Options;

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
            if (config.Path == null)
            {
                return IsMethodsMatch(id.Verb, config.Method);
            }

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
    }
}