using System;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using OpenApiServer.Core.MockServer.Context.Types;

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
    }
}