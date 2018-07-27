using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Context.Mapping;
using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Core.MockServer.Types;
using ITExpert.OpenApi.Utils;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Context
{
    public class ContextProvider
    {
        private ILogger Logger { get; }
        private IDictionary<RouteId, RequestContext> Contexts { get; }

        public IReadOnlyCollection<RouteId> Routes { get; }

        public ContextProvider(IEnumerable<OpenApiDocument> specs,
                               IOptionsMonitor<MockServerOptions> options,
                               ILoggerFactory loggerFactory)
        {
            Contexts = CreateContexts(options.CurrentValue, specs);
            Routes = Contexts.Keys.ToArray();

            options.OnChange(UpdateContexts);

            Logger = loggerFactory.CreateLogger("Context");
        }

        public RequestContext GetContext(HttpContext ctx)
        {
            var id = GetRouteId(ctx);
            return Contexts[id].WithRequest(ctx, Logger);
        }

        private void UpdateContexts(MockServerOptions options)
        {
            foreach (var (id, requestContext) in Contexts)
            {
                foreach (var routeConfig in options.Routes)
                {
                    if (!routeConfig.IsMatch(id))
                    {
                        continue;
                    }

                    var newValue = MapConfig(routeConfig, options.MockServerHost);
                    requestContext.UpdateConfig(newValue);
                }
            }
        }

        private static IDictionary<RouteId, RequestContext> CreateContexts(MockServerOptions options, IEnumerable<OpenApiDocument> specs)
        {
            var result = new Dictionary<RouteId, RequestContext>();

            foreach (var spec in specs)
            {
                foreach (var (path, pathSpec) in spec.Paths)
                {
                    foreach (var (verb, operation) in pathSpec.Operations)
                    {
                        var key = GetRouteId(spec, operation, verb, path);
                        var config = options.Routes.Where(x => x.IsMatch(key)).Aggregate((acc, x) => x.Merge(acc));
                        var configCtx = MapConfig(config, options.MockServerHost);
                        var specCtx = RequestContextSpecConverter.ConvertSpec(operation, spec.Servers);

                        var value = new RequestContext(configCtx, specCtx);
                        result[key] = value;
                    }
                }
            }

            return result;
        }

        private static RequestContextConfig MapConfig(MockServerRouteOptions option, string defaultHost)
        {
            return new RequestContextConfig
                   {
                           Delay = option.Delay,
                           Mock = option.Mock ?? false,
                           ValidateRequest = option.ShouldValidateRequest,
                           ValidateResponse = option.ShouldValidateResponse,
                           Host = UrlHelper.GetHost(option.Host, defaultHost)
                   };
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

        private static RouteId GetRouteId(OpenApiDocument spec,
                                          OpenApiOperation operation,
                                          OperationType operationType,
                                          string operationPath)
        {
            var server = operation.Servers.FirstOrDefault() ?? spec.Servers.FirstOrDefault();
            var prefix = server == null
                                 ? $"api/{spec.Info.GetServiceName()}"
                                 : UrlHelper.GetPathPrefix(server.FormatUrl());

            var route = UrlHelper.Join(prefix, operationPath);
            var method = ConvertVerb(operationType);
            return new RouteId(route, method);
        }

        private static HttpMethod ConvertVerb(OperationType type)
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
}