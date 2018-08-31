using Microsoft.Extensions.Logging;

using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Core.MockServer.Options;

namespace OpenApiServer.Core.MockServer.Context.Types
{
    public class RequestContext
    {
        public MockServerRouteOptions Config { get; }
        public RequestContextSpec Spec { get; }
        public RequestContextCall Request { get; }

        public IRequestHandler Handler { get; }

        public ILogger Logger { get; }

        public RequestContext(MockServerRouteOptions config,
                              RequestContextSpec spec,
                              RequestContextCall request,
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