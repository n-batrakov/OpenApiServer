using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
                                .Select(GetRouteOptions)
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

        private static bool IsRouteConfig(IConfiguration config, RouteId route)
        {
            var path = config.GetValue<string>("path");
            var method = config.GetValue<string>("method");
            if (path == null)
            {
                return IsMethodsMatch(method, route.Verb);
            }

            var regexp = new Regex(path);
            return regexp.IsMatch(route.Path) && IsMethodsMatch(method, route.Verb);

            bool IsMethodsMatch(string configMethod, HttpMethod routeMethod)
            {
                const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

                if (configMethod.Equals("any", comparison))
                {
                    return true;
                }

                var routeMethodString = routeMethod.ToString();
                return configMethod.Equals(routeMethodString, comparison);
            }
        }

        private static MockServerRouteOptions GetRouteOptions(IConfiguration routeConfig) =>
                new MockServerRouteOptions
                {
                        Path = routeConfig["path"],
                        Method = GetEnumValue<MockServerOptionsHttpMethod>(routeConfig, "method"),

                        Handler = routeConfig["handler"] ?? "default",
                        Config = routeConfig
                };

        private static MockServerRouteOptions GetDefaultRouteOptions(RouteId id) =>
                new MockServerRouteOptions
                {
                        Path = id.Path,
                        Method = ConvertMethod(id.Verb),
                        Handler = "default",
                        Config = new ConfigurationRoot(new List<IConfigurationProvider>()),
                };

        private static T GetEnumValue<T>(IConfiguration configuration, string key) where T : struct
        {
            var str = configuration[key];
            Enum.TryParse<T>(str, ignoreCase: true, out var result);
            return result;
        }

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