using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Configuration;

using OpenApiServer.Core.MockServer.Context.Internals;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Options;

using Xunit;
using Xunit.Sdk;

namespace UnitTests.ContextProviderTests
{
    public class RouteOptionsBuilderTests
    {
        [Fact]
        public void CanBuildRouteOptions()
        {
            var config = GetConfig("/", "get", "test");
            var route = new RouteId("/", HttpMethod.Get);

            var expected = new MockServerRouteOptions
                           {
                                   Path = "/",
                                   Method = MockServerOptionsHttpMethod.Get,
                                   Handler = "test"
                           };

            var actual = RouteOptionsBuilder.Build(route, config);

            AssertEqual(expected, actual);
        }

        [Fact]
        public void ReturnDefaultOptionsIfRouteIsNotConfigured()
        {
            var config = GetConfig("/", "get", "test");
            var route = new RouteId("/test", HttpMethod.Get);

            var expected = new MockServerRouteOptions
                           {
                                   Path = "/test",
                                   Method = MockServerOptionsHttpMethod.Get,
                                   Handler = "default"
                           };

            var actual = RouteOptionsBuilder.Build(route, config);

            AssertEqual(expected, actual);
        }

        [Fact]
        public void ReturnDefaultOptionsIfNoRoutesConfigured()
        {
            var config = GetConfig(new Dictionary<string, string>());
            var route = new RouteId("/", HttpMethod.Get);

            var expected = new MockServerRouteOptions
                           {
                                   Path = "/",
                                   Method = MockServerOptionsHttpMethod.Get,
                                   Handler = "default"
                           };

            var actual = RouteOptionsBuilder.Build(route, config);

            AssertEqual(expected, actual);
        }



        private static void AssertEqual(MockServerRouteOptions expected, MockServerRouteOptions actual)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;
            if (!string.Equals(expected.Path, actual.Path, comparison))
            {
                throw new XunitException(GetErrorMessage("path", expected.Path, actual.Path));
            }
            if (expected.Method != actual.Method)
            {
                throw new XunitException(GetErrorMessage("method", expected.Method, actual.Method));
            }
            if (!string.Equals(expected.Handler, actual.Handler, comparison))
            {
                throw new XunitException(GetErrorMessage("handler", expected.Handler, actual.Handler));
            }

            string GetErrorMessage(string name, object expectedValue, object actualValue) => 
                    $"Expected {name} is not equal to actual {name}.\r\nExpected: {expectedValue}\r\nActual: {actualValue}";
        }



        private static IConfiguration GetConfig(string path,
                                                string method,
                                                string handler,
                                                params (string Key, string Value)[] optionalParams)
        {
            var config = new Dictionary<string, string>
                         {
                                 ["path"] = path,
                                 ["method"] = method,
                                 ["handler"] = handler
                         };
            foreach (var (key, value) in optionalParams) config[key] = value;

            return GetConfig(config);
        }

        private static IConfiguration GetConfig(IDictionary<string, string> config)
        {
            var sectionValues = config.ToDictionary(x => $"routes:0:{x.Key}", x => x.Value);

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(sectionValues);
            return configBuilder.Build();
        }
    }
}