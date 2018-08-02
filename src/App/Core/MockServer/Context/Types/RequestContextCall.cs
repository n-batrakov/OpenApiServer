using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace ITExpert.OpenApi.Server.Core.MockServer.Context.Types
{
    public class RequestContextCall
    {
        public string PathAndQuery { get; set; }

        public HttpMethod Method { get; set; }

        public string Host { get; set; }

        public RouteData Route { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public IQueryCollection Query { get; set; }

        public string Body { get; set; }

        public string ContentType { get; set; }
    }
}