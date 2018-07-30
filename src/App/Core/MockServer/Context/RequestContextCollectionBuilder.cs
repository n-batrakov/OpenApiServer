using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Context.Mapping;
using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Utils;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Context
{
    public static class RequestContextCollectionBuilder
    {
        public static void UpdateContexts(IDictionary<RouteId, RequestContext> contexts, MockServerOptions options)
        {
            foreach (var (id, requestContext) in contexts)
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

        public static IDictionary<RouteId, RequestContext> Build(MockServerOptions options,
                                                                 IEnumerable<OpenApiDocument> specs)
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

        private static RouteId GetRouteId(OpenApiDocument spec,
                                          OpenApiOperation operation,
                                          OperationType operationType,
                                          string operationPath)
        {
            var server = operation.Servers.FirstOrDefault() ?? spec.Servers.FirstOrDefault();
            var prefix = server == null
                                 ? UrlHelper.GetDefaultPathPrefix(spec.Info.GetServiceName())
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