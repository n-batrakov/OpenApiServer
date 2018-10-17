using System.Collections.Generic;
using System.Net;

using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Types.Spec;

namespace UnitTests.Utils
{
    public class RouteSpecBuilder
    {
        private List<RouteSpecRequestParameter> Parameters { get; }
        private List<RouteSpecRequestBody> Bodies { get; }
        private List<RouteSpecResponse> Responses { get; }
        private List<string> Servers { get; }

        public RouteSpecBuilder()
        {
            Parameters = new List<RouteSpecRequestParameter>();
            Bodies = new List<RouteSpecRequestBody>();
            Responses = new List<RouteSpecResponse>();
            Servers = new List<string>();
        }

        public RouteSpec Build()
        {
            return new RouteSpec(Parameters, Bodies, Responses, Servers);
        }

        public RouteSpecBuilder WithServer(string url)
        {
            Servers.Add(url);
            return this;
        }

        public RouteSpecBuilder WithQueryParameter(string name, JSchema schema, bool required = false)
        {
            var param = new RouteSpecRequestParameter
                        {
                                In = ParameterLocation.Query,
                                Name = name,
                                Schema = schema,
                                Required = required,
                        };
            Parameters.Add(param);
            return this;
        }

        public RouteSpecBuilder WithBody(JSchema schema, bool required = false)
        {
            var body = new RouteSpecRequestBody("application/json", required, schema, new List<string>());
            Bodies.Add(body);
            return this;
        }

        public RouteSpecBuilder WithResponse(JSchema schema, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "application/json")
        {
            var statusCodeNum = (int)statusCode;
            var response = new RouteSpecResponse(contentType, statusCodeNum.ToString(), schema, new List<string>());
            Responses.Add(response);
            return this;
        }
    }
}