using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ITExpert.OpenApi.Server.Core.MockServer.Types
{
    public class HttpRequestValidationContext
    {
        public RouteData Route { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public IQueryCollection Query { get; set; }

        public string Body { get; set; }

        public IFormCollection Form { get; set; }

        public string ContentType { get; set; }
    }
}