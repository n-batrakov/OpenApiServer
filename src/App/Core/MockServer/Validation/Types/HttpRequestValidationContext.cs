using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public class HttpRequestValidationContext
    {
        public RouteData Route { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public IQueryCollection Query { get; set; }

        public string Body { get; set; }

        public string ContentType { get; set; }
    }
}