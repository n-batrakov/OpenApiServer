using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Context;
using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockServerBuilder
    {
        private IApplicationBuilder ApplicationBuilder { get; }
        private RequestContextProvider ContextProvider { get; }
        private ILogger Logger { get; }
        private IMockServerRequestHandler RequestHandler { get; }

        public MockServerBuilder(IApplicationBuilder app, IEnumerable<OpenApiDocument> specs)
        {
            ApplicationBuilder = app;
            ContextProvider = ActivatorUtilities.CreateInstance<RequestContextProvider>(app.ApplicationServices, specs);
            RequestHandler = app.ApplicationServices.GetRequiredService<IMockServerRequestHandler>();
            Logger = app.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger(nameof(MockServerBuilder)) ??
                     NullLogger.Instance;
        }

        public IRouter Build()
        {
            var builder = new RouteBuilder(ApplicationBuilder);
            foreach (var id in ContextProvider.Routes)
            {
                builder.MapVerb(id.Verb.ToString(), id.Path, HandleRequest);
            }

            return builder.Build();
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