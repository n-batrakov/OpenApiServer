using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Internals;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.MockServer
{
    public static class MockServerExtensions
    {
        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app, params OpenApiDocument[] specs)
        {
            var routes = specs.SelectMany(GetRoutes).ToArray();
            var routeBuilder = new RouteBuilder(app);

            foreach (var route in routes)
            {
                var template = GetRouteTemplate(route.Path);
                var handler = new MockRouteHandler(route.Operation, route.Validator, route.Generator);
                routeBuilder.MapVerb(template, route.OperationType.ToString(), handler.InvokeAsync);
            }

            return app.UseRouter(routeBuilder.Build());
        }

        // ReSharper disable once UnusedParameter.Local
        private static string GetRouteTemplate(string openApiRoute)
        {
            throw new NotImplementedException();
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