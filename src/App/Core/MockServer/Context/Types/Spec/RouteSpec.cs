using System.Collections.Generic;

namespace OpenApiServer.Core.MockServer.Context.Types.Spec
{
    public class RouteSpec
    {
        public IReadOnlyCollection<RouteSpecRequestParameter> Parameters { get; }
        public IReadOnlyCollection<RouteSpecRequestBody> Bodies { get; }
        public IReadOnlyCollection<RouteSpecResponse> Responses { get; }
        public IReadOnlyCollection<string> Servers { get; }

        public RouteSpec(IReadOnlyCollection<RouteSpecRequestParameter> parameters,
                               IReadOnlyCollection<RouteSpecRequestBody> bodies,
                               IReadOnlyCollection<RouteSpecResponse> responses,
                               IReadOnlyCollection<string> servers)
        {
            Parameters = parameters;
            Bodies = bodies;
            Responses = responses;
            Servers = servers;
        }
    }
}