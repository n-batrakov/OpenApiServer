using Microsoft.Extensions.Logging;

using OpenApiServer.Core.MockServer.Context.Types.Spec;
using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Core.MockServer.Options;

namespace OpenApiServer.Core.MockServer.Context.Types
{
    public class RouteContext
    {
        public MockServerRouteOptions Config { get; }
        public RouteSpec Spec { get; }
        public RequestContext Request { get; }

        public IRequestHandler Handler { get; }

        public ILogger Logger { get; }

        public RouteContext(
                MockServerRouteOptions config,
                RouteSpec spec,
                RequestContext request,
                IRequestHandler handler,
                ILogger logger)
        {
            Config = config;
            Spec = spec;
            Request = request;
            Handler = handler;
            Logger = logger;
        }
    }
}