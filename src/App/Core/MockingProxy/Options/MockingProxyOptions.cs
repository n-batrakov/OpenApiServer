using System.Collections.Generic;

namespace ITExpert.OpenApi.Server.Core.MockingProxy.Options
{
    public class MockingProxyOptions
    {
        public Dictionary<string, MockingProxyRouteOptions> Routes { get; set; } =
            new Dictionary<string, MockingProxyRouteOptions>();
    }
}