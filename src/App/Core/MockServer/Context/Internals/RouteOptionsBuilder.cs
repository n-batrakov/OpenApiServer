using System;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Configuration;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Options;

namespace OpenApiServer.Core.MockServer.Context.Internals
{
    internal static class RouteOptionsBuilder
    {
        public static MockServerRouteOptions Build(RouteId route, IConfiguration config)
        {
            return config.GetSection("routes")
                         .GetChildren()
                         .Where(x => IsRouteConfig(x, route))
                         .Select(GetRouteOptions)
                         .Aggregate(MergeRouteOptions);
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

        private static T GetEnumValue<T>(IConfiguration configuration, string key) where T : struct
        {
            var str = configuration[key];
            Enum.TryParse<T>(str, ignoreCase: true, out var result);
            return result;
        }
    }
}