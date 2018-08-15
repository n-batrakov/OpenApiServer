using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Core.MockServer.Options;

namespace OpenApiServer.Core.MockServer.Context.Types
{
    public class RequestContext
    {
        public MockServerRouteOptions Config { get; private set; }
        public RequestContextSpec Spec { get; private set; }
        public RequestContextCall Request { get; private set; }

        public IRequestHandler Handler { get; private set; }

        public ILogger Logger { get; private set; }

        private RequestContext()
        {
        }

        public RequestContext(MockServerRouteOptions config, RequestContextSpec spec)
        {
            Config = config;
            Spec = spec;
            Logger = NullLogger.Instance;
        }

        public RequestContext WithRequest(RequestContextCall call, IRequestHandler handler, ILogger logger = null) =>
                new RequestContext
                {
                        Spec = Spec,
                        Config = Config,
                        Handler = handler,
                        Request = call,
                        Logger = logger ?? Logger
                };

        public void UpdateConfig(MockServerRouteOptions config)
        {
            Config = config;
        }
    }
}