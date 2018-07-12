using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockRouteBuilder
    {
        private IEnumerable<OpenApiDocument> Specs { get; }
        private RouteBuilder RouteBuilder { get; }

        public MockRouteBuilder(IApplicationBuilder applicationBuilder, IEnumerable<OpenApiDocument> specs)
        {
            RouteBuilder = new RouteBuilder(applicationBuilder);
            Specs = specs;
        }

        public IRouter Build()
        {
            foreach (var spec in Specs)
            {
                MapSpec(spec);
            }

            return RouteBuilder.Build();
        }

        private void MapSpec(OpenApiDocument spec)
        {
            foreach (var route in GetRoutes(spec))
            {
                var template = GetRouteTemplate(route.Path);
                var handler = new MockRouteHandler(route.Operation, route.Validator, route.Generator);
                RouteBuilder.MapVerb(template, route.OperationType.ToString(), handler.InvokeAsync);
            }
        }

        private static string GetRouteTemplate(string openApiRoute)
        {
            return openApiRoute;
        }

        private static IEnumerable<MockServerRouteContext> GetRoutes(OpenApiDocument doc)
        {
            var validator = new RequestValidator();
            var generator = new ResponseGenerator(doc);

            return doc.Paths.SelectMany(
                    path => path.Value.Operations.Select(
                            verb => new MockServerRouteContext
                                    {
                                            Path = path.Key,
                                            Operation = verb.Value,
                                            OperationType = verb.Key,
                                            Validator = validator,
                                            Generator = generator
                                    }));
        }

        private class MockServerRouteContext
        {
            public string Path { get; set; }
            public OperationType OperationType { get; set; }
            public OpenApiOperation Operation { get; set; }
            public RequestValidator Validator { get; set; }
            public ResponseGenerator Generator { get; set; }
        }
    }
}