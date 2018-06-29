using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApiServer.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.MockServer
{
    public static class MockServerExtensions
    {
        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app, params OpenApiDocument[] specs)
        {
            var routes = specs.SelectMany(GetRoutes).ToArray();
            var validator = new RequestValidator();
            var generator = new ResponseGenerator();
            var routeBuilder = new RouteBuilder(app);

            foreach (var route in routes)
            {
                var template = GetRouteTemplate(route.Path);
                var handler = new MockRouteHandler(route.Operation, validator, generator, route.Resolver);
                routeBuilder.MapVerb(template, route.OperationType.ToString(), handler.InvokeAsync);
            }

            return app.UseRouter(routeBuilder.Build());
        }

        private static string GetRouteTemplate(string openApiRoute)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<MockServerRouteContext> GetRoutes(OpenApiDocument doc)
        {
            var resolver = new ReferenceResolver(doc);
            return doc.Paths.SelectMany(
                    path => path.Value.Operations.Select(
                            verb => new MockServerRouteContext(
                                    path.Key,
                                    verb.Key,
                                    verb.Value,
                                    resolver)));
        }

        private class MockServerRouteContext
        {
            public string Path { get; }
            public OperationType OperationType { get; }
            public OpenApiOperation Operation { get; }
            public ReferenceResolver Resolver { get; }

            public MockServerRouteContext(string path,
                                          OperationType operationType,
                                          OpenApiOperation operation,
                                          ReferenceResolver resolver)
            {
                Path = path;
                OperationType = operationType;
                Operation = operation;
                Resolver = resolver;
            }
        }
    }
}