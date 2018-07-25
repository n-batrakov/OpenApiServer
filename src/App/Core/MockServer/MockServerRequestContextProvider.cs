using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Core.MockServer.PathProviders;
using ITExpert.OpenApi.Server.Core.MockServer.Types;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockServerRequestContextProvider
    {
        public IEnumerable<RouteId> Routes { get; }

        private IOpenApiOperationPathProvider PathProvider { get; }

        private IDictionary<RouteId, OpenApiOperation> AvailableRoutes { get; }

        private IDictionary<RouteId, MockServerRouteOptions> RouteConfigs { get; set; }

        public MockServerRequestContextProvider(IOpenApiOperationPathProvider pathProvider,
                                                IOptionsMonitor<MockServerOptions> options,
                                                IEnumerable<OpenApiDocument> specs)
        {
            PathProvider = pathProvider;

            options.OnChange(OnOptionsChange);

            AvailableRoutes = GetAvailableRoutes(specs);
            Routes = AvailableRoutes.Keys;

            RouteConfigs = GetRoutesConfig(options.CurrentValue, AvailableRoutes);
        }

        public IMockServerRequestContext GetContext(HttpContext ctx)
        {
            var key = GetRouteId(ctx);

            var hasSpec = AvailableRoutes.TryGetValue(key, out var spec);
            if (!hasSpec)
            {
                throw new Exception($"Unable to find spec for {key}");
            }

            var hasConfig = RouteConfigs.TryGetValue(key, out var config);
            if (!hasConfig)
            {
                config = MockServerRouteOptions.Default;
            }

            return CreateRequestContext(ctx, spec, config);
        }

        private static RouteId GetRouteId(HttpContext ctx)
        {
            var routeData = ctx.GetRouteData();
            var route = routeData.Routers.OfType<Route>().FirstOrDefault();
            if (route == null)
            {
                throw new Exception($"Unable to find route for {ctx.Request.Path} ({ctx.Request.Method})");
            }

            var template = route.RouteTemplate;
            var verb = ctx.Request.Method.ToLowerInvariant();
            return new RouteId(template, verb);
        }

        private void OnOptionsChange(MockServerOptions options)
        {
            RouteConfigs = GetRoutesConfig(options, AvailableRoutes);
        }

        private static IDictionary<RouteId, MockServerRouteOptions> GetRoutesConfig(
                MockServerOptions options,
                IDictionary<RouteId, OpenApiOperation> availableRoutes)
        {
            var result = new Dictionary<RouteId, MockServerRouteOptions>();

            foreach (var routeConfig in options.Routes.Reverse())
            {
                var regexp = new Regex(routeConfig.Path);
                var matchedRoutes = availableRoutes.Keys.Where(x => regexp.IsMatch(x.Path)).ToArray();

                foreach (var id in matchedRoutes)
                {
                    if (!IsMethodsMatch(id.Verb, routeConfig.Method))
                    {
                        continue;
                    }

                    result[id] = routeConfig;
                }
            }

            return result;

            bool IsMethodsMatch(HttpMethod specMethod, MockServerOptionsHttpMethod configMethod)
            {
                switch (configMethod)
                {
                    case MockServerOptionsHttpMethod.Get:
                        return specMethod == HttpMethod.Get;
                    case MockServerOptionsHttpMethod.Put:
                        return specMethod == HttpMethod.Put;
                    case MockServerOptionsHttpMethod.Delete:
                        return specMethod == HttpMethod.Delete;
                    case MockServerOptionsHttpMethod.Post:
                        return specMethod == HttpMethod.Post;
                    case MockServerOptionsHttpMethod.Head:
                        return specMethod == HttpMethod.Head;
                    case MockServerOptionsHttpMethod.Trace:
                        return specMethod == HttpMethod.Trace;
                    case MockServerOptionsHttpMethod.Patch:
                        return specMethod == HttpMethod.Patch;
                    case MockServerOptionsHttpMethod.Connect:
                        return specMethod == HttpMethod.Connect;
                    case MockServerOptionsHttpMethod.Options:
                        return specMethod == HttpMethod.Options;
                    case MockServerOptionsHttpMethod.Any:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(configMethod), configMethod, null);
                }
            }
        }

        private IDictionary<RouteId, OpenApiOperation> GetAvailableRoutes(IEnumerable<OpenApiDocument> specs)
        {
            var result = new Dictionary<RouteId, OpenApiOperation>();

            foreach (var spec in specs)
            {
                foreach (var (path, routeSpec) in spec.Paths)
                {
                    foreach (var (verb, operation) in routeSpec.Operations)
                    {
                        var route = PathProvider.GetPath(spec, operation, path).ToLowerInvariant();
                        var method = ConvertVerb(verb);
                        var key = new RouteId(route, method);
                        var value = operation;

                        if (result.ContainsKey(key))
                        {
                            throw new Exception(
                                    $"Unable to map path {path} ({verb}) " +
                                    $"from {spec.Info.Title} ({spec.Info.Version}) " +
                                    "because it overrides path from another spec. " +
                                    "Try configuring route template to resolve the ambiguity.");
                        }

                        result[key] = value;
                    }
                }
            }

            return result;

            HttpMethod ConvertVerb(OperationType type)
            {
                switch (type)
                {
                    case OperationType.Get:
                        return HttpMethod.Get;
                    case OperationType.Put:
                        return HttpMethod.Put;
                    case OperationType.Post:
                        return HttpMethod.Post;
                    case OperationType.Delete:
                        return HttpMethod.Delete;
                    case OperationType.Options:
                        return HttpMethod.Options;
                    case OperationType.Head:
                        return HttpMethod.Head;
                    case OperationType.Patch:
                        return HttpMethod.Patch;
                    case OperationType.Trace:
                        return HttpMethod.Trace;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }

        private static IMockServerRequestContext CreateRequestContext(HttpContext ctx,
                                                                      OpenApiOperation operation,
                                                                      MockServerRouteOptions routeOptions)
        {
            return new MockServerRequestContext
                   {
                           PathAndQuery = ctx.Request.Path,
                           Method = Enum.Parse<HttpMethod>(ctx.Request.Method, ignoreCase: true),
                           ContentType = ctx.Request.ContentType,
                           Query = ctx.Request.Query,
                           Headers = ctx.Request.Headers,
                           Route = ctx.GetRouteData(),
                           Body = ctx.Request.HasFormContentType ? ReadForm() : ReadBody(),
                           Options = routeOptions,
                           OperationSpec = operation
                   };

            string ReadForm()
            {
                var dict = ctx.Request.Form.ToDictionary(x => x.Key, x => JToken.Parse(x.Value));
                return JsonConvert.SerializeObject(dict);
            }

            string ReadBody()
            {
                using (var reader = new StreamReader(ctx.Request.Body))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private class MockServerRequestContext : IMockServerRequestContext
        {
            public string PathAndQuery { get; set; }

            public HttpMethod Method { get; set; }

            public RouteData Route { get; set; }

            public IHeaderDictionary Headers { get; set; }

            public IQueryCollection Query { get; set; }

            public string Body { get; set; }

            public string ContentType { get; set; }

            public OpenApiOperation OperationSpec { get; set; }

            public MockServerRouteOptions Options { get; set; }
        }
    }
}