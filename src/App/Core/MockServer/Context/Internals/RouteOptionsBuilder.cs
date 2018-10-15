using System;
using System.Collections.Generic;
using System.Linq;

using DotNet.Globbing;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Configuration;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Options;

namespace OpenApiServer.Core.MockServer.Context.Internals
{
    public static class RouteOptionsBuilder
    {
        public static MockServerRouteOptions Build(RouteId route, IConfiguration config)
        {
            var options = config.GetSection("routes")
                                .GetChildren()
                                .Where(x => IsRouteConfig(x, route))
                                .Select(x => GetRouteOptions(x, route))
                                .ToArray();

            return options.Length == 0
                           ? GetDefaultRouteOptions(route)
                           : options.Aggregate(MergeRouteOptions);
        }

        private static MockServerRouteOptions MergeRouteOptions(MockServerRouteOptions target,
                                                                MockServerRouteOptions source)
        {
            return new MockServerRouteOptions
                   {
                           Path = target.Path,
                           Method = target.Method,
                           Handler = target.Handler ?? source.Handler,
                           Config = target.Config ?? source.Config
                   };
        }

        /// <remarks>
        /// Default values for `path` and `method` is `*, any`.
        /// I.e. config without path and\or method always match.
        /// </remarks>
        private static bool IsRouteConfig(IConfiguration config, RouteId route)
        {
            var path = config.GetValue<string>("path");
            var method = config.GetValue<string>("method");
            if (path == null)
            {
                return IsMethodsMatch(method, route.Verb);
            }

            var glob = Glob.Parse(path);
            return glob.IsMatch(route.Path) && IsMethodsMatch(method, route.Verb);

            bool IsMethodsMatch(string configMethod, HttpMethod routeMethod)
            {
                if (string.IsNullOrEmpty(configMethod))
                {
                    return true;
                }

                if (configMethod.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var routeMethodString = routeMethod.ToString();
                return configMethod.Equals(routeMethodString, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static MockServerRouteOptions GetRouteOptions(IConfiguration routeConfig, RouteId route) =>
                new MockServerRouteOptions
                {
                        Path = route.Path,
                        Method = ConvertMethod(route.Verb),

                        Handler = routeConfig["handler"] ?? "default",
                        Config = routeConfig
                };

        private static MockServerRouteOptions GetDefaultRouteOptions(RouteId route) =>
                new MockServerRouteOptions
                {
                        Path = route.Path,
                        Method = ConvertMethod(route.Verb),
                        Handler = "default",
                        Config = new ConfigurationRoot(new List<IConfigurationProvider>()),
                };

        private static MockServerOptionsHttpMethod ConvertMethod(HttpMethod httpMethod)
        {
            switch (httpMethod)
            {
                case HttpMethod.Get:
                    return MockServerOptionsHttpMethod.Get;
                case HttpMethod.Put:
                    return MockServerOptionsHttpMethod.Put;
                case HttpMethod.Delete:
                    return MockServerOptionsHttpMethod.Delete;
                case HttpMethod.Post:
                    return MockServerOptionsHttpMethod.Post;
                case HttpMethod.Head:
                    return MockServerOptionsHttpMethod.Head;
                case HttpMethod.Trace:
                    return MockServerOptionsHttpMethod.Trace;
                case HttpMethod.Patch:
                    return MockServerOptionsHttpMethod.Patch;
                case HttpMethod.Connect:
                    return MockServerOptionsHttpMethod.Connect;
                case HttpMethod.Options:
                    return MockServerOptionsHttpMethod.Options;
                case HttpMethod.Custom:
                    return MockServerOptionsHttpMethod.Any;
                case HttpMethod.None:
                    return MockServerOptionsHttpMethod.Any;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpMethod), httpMethod, null);
            }
        }
    }
}