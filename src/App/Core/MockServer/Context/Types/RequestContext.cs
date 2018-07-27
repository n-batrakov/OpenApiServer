using ITExpert.OpenApi.Server.Core.MockServer.Types;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ITExpert.OpenApi.Server.Core.MockServer.Context.Types
{
    public class RequestContext
    {
        public RequestContextConfig Config { get; private set; }
        public RequestContextSpec Spec { get; }
        public RequestContextCall Request { get; }

        public ILogger Logger { get; private set; }

        public RequestContext(RequestContextConfig config, RequestContextSpec spec)
        {
            Config = config;
            Spec = spec;
            Logger = NullLogger.Instance;
        }

        public RequestContext(RequestContextConfig config,
                              RequestContextSpec spec,
                              RequestContextCall request)
        {
            Config = config;
            Spec = spec;
            Request = request;
            Logger = NullLogger.Instance;
        }

        public RequestContext WithRequest(RequestContextCall call, ILogger logger = null) =>
                new RequestContext(Config, Spec, call) {Logger = logger ?? Logger};

        public void UpdateConfig(RequestContextConfig config)
        {
            Config = config;
        }
    }
}