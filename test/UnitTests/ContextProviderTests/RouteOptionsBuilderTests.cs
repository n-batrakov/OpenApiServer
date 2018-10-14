using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Configuration;

using OpenApiServer.Core.MockServer.Context.Internals;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Options;

using Xunit;

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

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static void AssertEqual(MockServerRouteOptions expected, MockServerRouteOptions actual)
        {
            Assert.Equal(expected.Path, actual.Path);
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.Handler, actual.Handler);
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
            var sectionValues = config.ToDictionary(x => $"routes:0:{x}", x => x.Value);

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(sectionValues);
            return configBuilder.Build();
        }
    }
}