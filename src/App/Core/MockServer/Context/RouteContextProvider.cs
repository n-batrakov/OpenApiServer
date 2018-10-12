using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using OpenApiServer.Core.MockServer.Context.Internals;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Context.Types.Spec;
using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Server.Logging;

namespace OpenApiServer.Core.MockServer.Context
{
    public class RouteContextProvider
    {
        private ILogger Logger { get; }
        private IConfiguration Config { get; }
        private RequestHandlerProvider HandlerProvider { get; }
        private RouteSpecCollection Specs { get; }
        public IReadOnlyCollection<RouteId> Routes { get; }

        public RouteContextProvider(IEnumerable<OpenApiDocument> specs,
                                    IConfiguration config,
                                    RequestHandlerProvider handlerProvider,
                                    ILoggerFactory loggerFactory)
        {
            Config = config;
            Logger = loggerFactory.CreateOpenApiLogger();
            HandlerProvider = handlerProvider;

            Specs = new RouteSpecCollection(specs);
            Routes = Specs.Routes.ToArray();
        }

        public RouteContext GetContext(HttpContext ctx) =>
                GetContext(ctx.GetRouteId(), ctx);

        public RouteContext GetContext(RouteId id, HttpContext ctx)
        {
            var spec = Specs[id];
            var options = RouteOptionsBuilder.Build(id, Config);
            var callCtx = RequestContextBuilder.Build(ctx);
            var handler = HandlerProvider.GetHandler(options.Handler, options.Config, responseContext: null);

            return new RouteContext(options, spec, callCtx, handler, Logger);
        }
    }
}