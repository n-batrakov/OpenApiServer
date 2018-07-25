using ITExpert.OpenApi.Server.Core.MockServer.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public interface IMockServerRequestContext
    {
        string PathAndQuery { get; }
        HttpMethod Method { get; }

        RouteData Route { get; }
        IHeaderDictionary Headers { get; }
        IQueryCollection Query { get; }
        string Body { get; }
        string ContentType { get; }
        OpenApiOperation OperationSpec { get; }
        MockServerRouteOptions Options { get; }
    }
}