using System.Collections.Generic;

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

        public RouteSpecBuilder()
        {
            Parameters = new List<RouteSpecRequestParameter>();
            Bodies = new List<RouteSpecRequestBody>();
            Responses = new List<RouteSpecResponse>();
        }

        public RouteSpec Build()
        {
            return new RouteSpec(Parameters, Bodies, Responses, new string[0]);
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

        public RouteSpecBuilder WithResponse(JSchema schema, int statusCode = 200, string example = null)
        {
            var examples = example == null ? new List<string>() : new List<string> {example};
            var response = new RouteSpecResponse("application/json", statusCode.ToString(), schema, examples);
            Responses.Add(response);
            return this;
        }
    }
}