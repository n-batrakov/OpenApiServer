using System;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Context;
using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockServerBuilder
    {
        private ContextProvider ContextProvider { get; }
        private ILogger Logger { get; }
        private IMockServerRequestHandler RequestHandler { get; }

        public MockServerBuilder(ContextProvider contextProvider,
                                 IMockServerRequestHandler requestHandler,
                                 ILoggerFactory loggerFactory)
        {
            ContextProvider = contextProvider;
            RequestHandler = requestHandler;
            Logger = loggerFactory.CreateLogger(nameof(MockServerBuilder));
        }

        public IRouteBuilder MapMockServerRoutes(IRouteBuilder builder)
        {
            foreach (var id in ContextProvider.Routes)
            {
                builder.MapVerb(id.Verb.ToString(), id.Path, HandleRequest);
            }

            return builder;
        }

        private async Task HandleRequest(HttpContext ctx)
        {
            var requestContext = ContextProvider.GetContext(ctx);
            ctx.Features.Set(requestContext);

            try
            {
                var response = await RequestHandler.HandleAsync(requestContext).ConfigureAwait(false);
                await WriteResponse(ctx.Response, response).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await WriteException(ctx.Response, e);
                Logger.LogError(e, "An exception occured while handling request.");
            }
        }

        private static Task WriteResponse(HttpResponse response, MockServerResponseContext responseContext)
        {
            response.StatusCode = (int)responseContext.StatusCode;
            response.ContentType = responseContext.ContentType;

            foreach (var (key, value) in responseContext.Headers)
            {
                response.Headers[key] = value;
            }

            return response.WriteAsync(responseContext.Body);
        }

        private static Task WriteException(HttpResponse response, Exception exception)
        {
            response.StatusCode = 500;
            response.ContentType = "text/plain";
            var msg = $"[{GetExceptionPrettyName(exception)}]: {exception.Message}";
            return response.WriteAsync(msg, Encoding.UTF8);
        }

        private static string GetExceptionPrettyName(Exception e)
        {
            var fullName = e.GetType().Name;
            if (fullName == "Exception")
            {
                return "Generic";
            }

            if (fullName.EndsWith("Exception", StringComparison.Ordinal))
            {
                const int exceptionPostfixLength = 9;
                return fullName.Substring(0, fullName.Length - exceptionPostfixLength);
            }

            return fullName;
        }
    }
}