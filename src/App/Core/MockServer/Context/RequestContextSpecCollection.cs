using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.OpenApi.Models;

using OpenApiServer.Core.MockServer.Context.Mapping;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Utils;

namespace OpenApiServer.Core.MockServer.Context
{
    public class RequestContextSpecCollection
    {
        private IDictionary<RouteId, RequestContextSpec> Source { get; }

        public IEnumerable<RouteId> Routes => Source.Keys;
        public RequestContextSpec this[RouteId key] => Source[key];

        public RequestContextSpecCollection(IEnumerable<OpenApiDocument> specs)
        {
            Source = GetRouteSpecs(specs);
        }

        private static IDictionary<RouteId, RequestContextSpec> GetRouteSpecs(IEnumerable<OpenApiDocument> specs)
        {
            var result = new Dictionary<RouteId, RequestContextSpec>();

            foreach (var spec in specs)
            {
                foreach (var (path, pathSpec) in spec.Paths)
                {
                    foreach (var (verb, operation) in pathSpec.Operations)
                    {
                        var key = GetRouteId(spec, operation, verb, path);
                        var specCtx = RequestContextSpecConverter.ConvertSpec(operation, spec.Servers);
                        result[key] = specCtx;
                    }
                }
            }

            return result;
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