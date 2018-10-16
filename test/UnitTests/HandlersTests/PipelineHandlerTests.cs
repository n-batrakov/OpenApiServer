using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Exceptions;
using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Core.MockServer.Handlers.Defaults;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.HandlersTests
{
    public class PipelineHandlerTests
    {
        private static readonly RouteContext RouteContext = RouteContextBuilder.FromUrl("/").Build();

        [Fact]
        public void CanRunPipelineWithOneHandler()
        {
            var sut = Sut(typeof(TestHandler1));

            var expected = TestHandler1.Response;

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanRunPipelineWithMultipleHandlers()
        {
            var sut = Sut(typeof(TestHandler1), typeof(TestHandler2));

            var expected = TestHandler2.Response;

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanDisablePipelineStep()
        {
            var handlers = new HandlersCollection(typeof(TestHandler1), typeof(TestHandler2));
            var config = new InMemoryConfiguration("pipeline",
                                                   ("0:handler", "test1"),
                                                   ("1:disable", "true"),
                                                   ("1:handler", "test2"));
            var sut = new PipelineHandler(config, handlers.HandlerProvider);

            var expected = TestHandler1.Response;

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public async Task ThrowOnUnknownHandler()
        {
            var handlers = new HandlersCollection(typeof(TestHandler1));
            var config = new InMemoryConfiguration("pipeline", ("0:handler", "unknown"));
            var sut = new PipelineHandler(config, handlers.HandlerProvider);

            await Assert.ThrowsAsync<HandlerNotFoundException>(() => sut.HandleAsync(RouteContext));
        }

        [Fact]
        public void AllowHandlerToReturnNull()
        {
            var sut = Sut(typeof(TestHandlerNull));

            var expected = new ResponseContext();

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanBreakPipeline()
        {
            var sut = Sut(typeof(TestHandlerBreaks), typeof(TestHandler1));

            var expected = TestHandlerBreaks.Response;

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanMergeResponses()
        {
            var sut = Sut(typeof(TestHandlerOverwrite1), typeof(TestHandlerOverwrite2));

            var expected = TestHandlerOverwrite2.Response;
            expected.Body = "Test1";
            expected.Headers.Add("x-header1", "test");

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanPassPreviousResponseToHandler()
        {
            var sut = Sut(typeof(TestHandler1), typeof(TestHandlerWithPreviousResponse));

            var expected = TestHandler1.Response;

            var actual = sut.HandleAsync(RouteContext).Result;

            DeepAssert.Equal(expected, actual);
        }


        private static PipelineHandler Sut(params Type[] handlers)
        {
            var handlerCollection = new HandlersCollection(handlers);
            var configValues = handlerCollection.Keys.Select((x, i) => ($"{i}:handler", x)).ToArray();
            var config = new InMemoryConfiguration("pipeline", configValues);
            return new PipelineHandler(config, handlerCollection.HandlerProvider);
        }

        #region Test Types

        [RequestHandler("test1")]
        private class TestHandler1 : IRequestHandler
        {
            public static ResponseContext Response =>
                    new ResponseContext
                    {
                        Body = "Test1"
                    };

            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(Response);
        }

        [RequestHandler("test2")]
        private class TestHandler2 : IRequestHandler
        {
            public static ResponseContext Response =>
                    new ResponseContext
                    {
                        Body = "Test2"
                    };

            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(Response);
        }

        [RequestHandler("null")]
        private class TestHandlerNull : IRequestHandler
        {
            public Task<ResponseContext> HandleAsync(RouteContext request) => Task.FromResult<ResponseContext>(null);
        }

        [RequestHandler("breaks")]
        private class TestHandlerBreaks : IRequestHandler
        {
            public static ResponseContext Response =>
                    new ResponseContext
                    {
                        Body = "Breaks",
                        BreakPipeline = true
                    };

            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(Response);
        }

        [RequestHandler("overwrite1")]
        private class TestHandlerOverwrite1 : IRequestHandler
        {
            public static ResponseContext Response =>
                    new ResponseContext
                    {
                        Headers = new Dictionary<string, StringValues> { { "x-header1", "test" } },
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Body = "Test1"
                    };

            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(Response);
        }

        [RequestHandler("overwrite2")]
        private class TestHandlerOverwrite2 : IRequestHandler
        {
            public static ResponseContext Response =>
                    new ResponseContext
                    {
                        Headers = new Dictionary<string, StringValues> { { "x-header2", "test2" } },
                        StatusCode = HttpStatusCode.BadRequest,
                        ContentType = "text/html"
                    };

            public Task<ResponseContext> HandleAsync(RouteContext request) =>
                    Task.FromResult(Response);
        }

        [RequestHandler("prev")]
        private class TestHandlerWithPreviousResponse : IRequestHandler
        {
            public ResponseContext PreviousResponse { get; }

            public TestHandlerWithPreviousResponse(ResponseContext previousResponse)
            {
                PreviousResponse = previousResponse;
            }

            public Task<ResponseContext> HandleAsync(RouteContext request)
            {
                return Task.FromResult(PreviousResponse);
            }
        }

        #endregion
    }
}