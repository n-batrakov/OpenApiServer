using System.Collections.Generic;

namespace OpenApiServer.Core.MockServer.Context.Types
{
    public class RequestContextSpec
    {
        public IReadOnlyCollection<RequestContextParameter> Parameters { get; }
        public IReadOnlyCollection<RequestContextBody> Bodies { get; }
        public IReadOnlyCollection<RequestContextResponse> Responses { get; }
        public IReadOnlyCollection<string> Servers { get; }

        public RequestContextSpec(IReadOnlyCollection<RequestContextParameter> parameters,
                                  IReadOnlyCollection<RequestContextBody> bodies,
                                  IReadOnlyCollection<RequestContextResponse> responses,
                                  IReadOnlyCollection<string> servers)
        {
            Parameters = parameters;
            Bodies = bodies;
            Responses = responses;
            Servers = servers;
        }
    }
}