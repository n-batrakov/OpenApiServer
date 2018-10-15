using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using OpenApiServer.Core.MockServer.Context;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Exceptions;
using OpenApiServer.Core.MockServer.Handlers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.ContextProviderTests
{
    public class RequestHandlerProviderTests
    {
        private static RequestHandlerProvider Sut<T>() where T : IRequestHandler
        {
            var type = typeof(T);
            var name = type.GetCustomAttribute<RequestHandlerAttribute>().HandlerId;

            var handlers = new Dictionary<string, Type> {{name, type}};

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(new TestService {Test = "Test"});
            var serviceProvider = serviceCollection.BuildServiceProvider();

            return new RequestHandlerProvider(handlers, serviceProvider);
        }

        [Fact]
        public void CanGetHandler()
        {
            var expected = new TestHandler();

            var actual = Sut<TestHandler>().GetHandler("test", new InMemoryConfiguration(), new ResponseContext());

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGetHandlerWithOptions()
        {
            var config = new InMemoryConfiguration("", ("test", "42"));
            var expected = new TestHandlerWithOptions(new TestOptions {Test = "42"});

            var actual = Sut<TestHandlerWithOptions>().GetHandler("test", config, null);

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGetHandlerWithServices()
        {
            var expected = new TestHandlerWithServices(new TestService {Test = "Test"});

            var actual = Sut<TestHandlerWithServices>().GetHandler("test", null, null);

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGetHandlerWithResponseContext()
        {
            var responseContext = new ResponseContext {Body = "Test"};
            var expected = new TestHandlerWithResponseContext(responseContext);

            var actual = Sut<TestHandlerWithResponseContext>().GetHandler("test", null, responseContext);

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGetHandlerWithDefaultArguments()
        {
            var expected = new TestHandlerWithMultipleArguments(null, null, null, 0);

            var actual = Sut<TestHandlerWithMultipleArguments>().GetHandler("test", null, null);

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void ThrowIfHandlerIsUnknown()
        {
            Assert.Throws<HandlerNotFoundException>(() => Sut<TestHandler>().GetHandler("unknown", null, null));
        }
        

        #region TestTypes
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper disable UnusedMember.Local

        [RequestHandler("test")]
        private class TestHandler : IRequestHandler
        {
            public Task<ResponseContext> HandleAsync(RouteContext request) => throw new NotImplementedException();
        }

        [RequestHandler("test", typeof(TestOptions))]
        private class TestHandlerWithOptions : IRequestHandler
        {
            public TestOptions Config { get; }
            public TestHandlerWithOptions(TestOptions config) => Config = config;
            public Task<ResponseContext> HandleAsync(RouteContext request) => throw new NotImplementedException();
        }

        [RequestHandler("test")]
        private class TestHandlerWithServices : IRequestHandler
        {
            public TestService Service { get; }
            public TestHandlerWithServices(TestService service) => Service = service;
            public Task<ResponseContext> HandleAsync(RouteContext request) => throw new NotImplementedException();
        }

        [RequestHandler("test")]
        private class TestHandlerWithResponseContext : IRequestHandler
        {
            public ResponseContext ResponseContext { get; }
            public TestHandlerWithResponseContext(ResponseContext responseContext) => ResponseContext = responseContext;
            public Task<ResponseContext> HandleAsync(RouteContext request) => throw new NotImplementedException();
        }

        [RequestHandler("test", typeof(TestOptions))]
        private class TestHandlerWithMultipleArguments : IRequestHandler
        {
            public ResponseContext ResponseContext { get; }
            public UnregisteredService Service { get; }
            public TestOptions Options { get; }
            public int SomethingElse { get; }

            public TestHandlerWithMultipleArguments(ResponseContext responseContext,
                                                    UnregisteredService service,
                                                    TestOptions options,
                                                    int somethingElse)
            {
                ResponseContext = responseContext;
                Service = service;
                Options = options;
                SomethingElse = somethingElse;
            }

            public Task<ResponseContext> HandleAsync(RouteContext request)
            {
                throw new NotImplementedException();
            }
        }


        public class TestOptions
        {
            public string Test { get; set; }
        }

        private class TestService
        {
            public string Test { get; set; }
        }

        private class UnregisteredService
        {
            public string Test { get; set; }
        }

        // ReSharper restore UnusedMember.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        #endregion
    }
}