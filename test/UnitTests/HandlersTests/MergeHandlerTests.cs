using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenApiServer.Core.MockServer.Context;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Exceptions;
using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Core.MockServer.Handlers.Defaults;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.HandlersTests
{
    public class MergeHandlerTests
    {
        [Fact]
        public void CanMergeResponses()
        {
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            var expected = "{a: 42, b: 56}";

            var actual = Sut(typeof(TestHandler1), typeof(TestHandler2)).HandleAsync(ctx).Result?.Body;

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanDisableOneSourceWithConfig()
        {
            var config = new InMemoryConfiguration("handlers",
                                                   ("0:handler", "test1"),
                                                   ("1:handler", "test2"),
                                                   ("1:disable", "true"));
            var sut = Sut(config, typeof(TestHandler1), typeof(TestHandler2));
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            var expected = "{a: 42}";

            var actual = sut.HandleAsync(ctx).Result?.Body;

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public async Task ThrowOnUnknownHandler()
        {
            var config = new InMemoryConfiguration("handlers", ("0:handler", "test2"));
            var sut = Sut(config, typeof(TestHandler1));
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            await Assert.ThrowsAsync<HandlerNotFoundException>(() => sut.HandleAsync(ctx));
        }

        [Fact]
        public void ReturnNullWhenNoHandlersConfigured()
        {
            var config = new InMemoryConfiguration();
            var sut = Sut(config, typeof(TestHandler1));
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            var actual = sut.HandleAsync(ctx).Result;

            Assert.Null(actual);
        }

        [Fact]
        public void ReturnLastHandlerResultIfContentTypesNotEqual()
        {
            var sut = Sut(typeof(TestHandler1), typeof(TestHandlerPlainText1));
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            var expected = "Test1";

            var actual = sut.HandleAsync(ctx).Result.Body; 

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnLastHandlerResultIfContentTypesAreNotJson()
        {
            var sut = Sut(typeof(TestHandlerPlainText1), typeof(TestHandlerPlainText2));
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            var expected = "Test2";

            var actual = sut.HandleAsync(ctx).Result.Body;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnLastHandlerResultIfStatusCodesNotEqual()
        {
            var sut = Sut(typeof(TestHandler1), typeof(TestHandlerUnauthorized));
            var ctx = RouteContextBuilder.FromUrl("/").Build();

            var expected = "{ success: false }";

            var actual = sut.HandleAsync(ctx).Result?.Body;

            JsonAssert.Equal(expected, actual);
        }




        private static MergeHandler Sut(params Type[] handlerTypes)
        {
            var handlerMap = handlerTypes.ToDictionary(x => x.GetCustomAttribute<RequestHandlerAttribute>().HandlerId);
            var handlerProvider = CreateHandlerProvider(handlerMap);

            var configValues = handlerMap.Keys.Select((x, i) => ($"handlers:{i}:handler", x)).ToArray();
            var config = new InMemoryConfiguration("", configValues);

            return new MergeHandler(config, handlerProvider);
        }

        private static MergeHandler Sut(IConfiguration config, params Type[] handlers)
        {
            var handlerMap = GetHandlerMap(handlers);
            var handlerProvider = CreateHandlerProvider(handlerMap);
            return new MergeHandler(config, handlerProvider);
        }

        private static IDictionary<string, Type> GetHandlerMap(params Type[] handlerTypes) =>
                handlerTypes.ToDictionary(x => x.GetCustomAttribute<RequestHandlerAttribute>().HandlerId);

        private static RequestHandlerProvider CreateHandlerProvider(IDictionary<string, Type> handlerMap)
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            return new RequestHandlerProvider(handlerMap, serviceProvider);
        }

        [RequestHandler("test1")]
        private class TestHandler1 : IRequestHandler
        {
            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(new ResponseContext
                                    {
                                            ContentType = "application/json",
                                            Body = "{a: 42}"
                                    });
        }

        [RequestHandler("test2")]
        private class TestHandler2 : IRequestHandler
        {
            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(new ResponseContext
                                    {
                                            ContentType = "application/json",
                                            Body = "{b: 56}"
                                    });
        }

        [RequestHandler("test31")]
        private class TestHandlerPlainText1 : IRequestHandler
        {
            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(new ResponseContext
                                    {
                                            ContentType = "text/plain",
                                            Body = "Test1"
                                    });
        }

        [RequestHandler("test32")]
        private class TestHandlerPlainText2 : IRequestHandler
        {
            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(new ResponseContext
                                    {
                                            ContentType = "text/plain",
                                            Body = "Test2"
                                    });
        }

        [RequestHandler("test4")]
        private class TestHandlerUnauthorized : IRequestHandler
        {
            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(new ResponseContext
                                    {
                                            ContentType = "application/json",
                                            StatusCode = HttpStatusCode.Unauthorized,
                                            Body = "{success: false}"
                                    });
        }
    }
}