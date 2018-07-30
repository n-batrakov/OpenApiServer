using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Server.Core.MockServer.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Context
{
    public class RequestContextProvider
    {
        private ILogger Logger { get; }
        private IDictionary<RouteId, RequestContext> Contexts { get; }

        public IReadOnlyCollection<RouteId> Routes { get; }

        public RequestContextProvider(IEnumerable<OpenApiDocument> specs,
                               IOptionsMonitor<MockServerOptions> options,
                               ILoggerFactory loggerFactory)
        {
            Contexts = RequestContextCollectionBuilder.Build(options.CurrentValue, specs);
            Routes = Contexts.Keys.ToArray();

            options.OnChange(OnOptionsChange);

            Logger = loggerFactory.CreateLogger("Context");
        }

        public RequestContext GetContext(HttpContext ctx)
        {
            var id = GetRouteId(ctx);
            return Contexts[id].WithRequest(ctx, Logger);
        }

        private void OnOptionsChange(MockServerOptions options)
        {
            RequestContextCollectionBuilder.UpdateContexts(Contexts, options);
        }

        private static RouteId GetRouteId(HttpContext ctx)
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
    }
}