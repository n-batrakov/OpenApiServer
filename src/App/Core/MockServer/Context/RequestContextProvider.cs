using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Server.Logging;

namespace OpenApiServer.Core.MockServer.Context
{
    public class RequestContextProvider
    {
        private ILogger Logger { get; }
        private IConfiguration Config { get; }
        private IRequestHandlerProvider HandlerProvider { get; }

        private RequestContextSpecCollection Specs { get; }
        public IReadOnlyCollection<RouteId> Routes { get; }

        public RequestContextProvider(IEnumerable<OpenApiDocument> specs,
                                      IConfiguration config,
                                      IRequestHandlerProvider handlerProvider,
                                      ILoggerFactory loggerFactory)
        {
            Config = config;
            Logger = loggerFactory.CreateOpenApiLogger();
            HandlerProvider = handlerProvider;

            Specs = new RequestContextSpecCollection(specs);
            Routes = Specs.Routes.ToArray();
        }

        public RequestContext GetContext(HttpContext ctx) =>
                GetContext(ctx.GetRouteId(), ctx);

        public RequestContext GetContext(RouteId id, HttpContext ctx)
        {
            var spec = Specs[id];
            var options = RouteOptionsBuilder.Build(id, Config);
            var callCtx = RequestContextCallBuilder.GetCallContext(ctx);
            var handler = HandlerProvider.GetHandler(options.Handler, options.Config);

            return new RequestContext(options, spec, callCtx, handler, Logger);
        }
    }
}