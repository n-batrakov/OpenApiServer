using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using OpenApiServer.Core.MockServer.Context;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Server.Logging;

namespace OpenApiServer.Core.MockServer
{
    public class MockServerBuilder
    {
        private IApplicationBuilder ApplicationBuilder { get; }

        private RequestContextProvider ContextProvider { get; }

        private ILogger Logger { get; }

        public MockServerBuilder(IApplicationBuilder app, IEnumerable<OpenApiDocument> specs)
        {
            ApplicationBuilder = app;
            ContextProvider = ActivatorUtilities.CreateInstance<RequestContextProvider>(app.ApplicationServices, specs);
            
            Logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateOpenApiLogger();
        }

        public IRouter Build()
        {
            var builder = new RouteBuilder(ApplicationBuilder);
            foreach (var id in ContextProvider.Routes)
            {
                var verb = id.Verb.ToString();

                builder.MapVerb(verb, id.Path, HandleGeneralRequest);
                builder.MapVerb(verb, $"/mock/{id.Path}", HandleMockRequest);
            }

            return builder.Build();
        }

        private Task HandleMockRequest(HttpContext ctx)
        {
            var mockId = ctx.GetRouteId();

            var path = mockId.Path.Substring(6);
            var id = new RouteId(path, mockId.Verb);

            var requestContext = ContextProvider.GetContext(id, ctx);
            requestContext.Config.Handler = "mock";

            return HandleRequest(requestContext, ctx.Response);
        }

        private Task HandleGeneralRequest(HttpContext ctx)
        {
            var requestContext = ContextProvider.GetContext(ctx);
            ctx.Features.Set(requestContext);

            return HandleRequest(requestContext, ctx.Response);
        }        

        private Task HandleRequest(RequestContext requestContext, HttpResponse httpResponse)
        {
            return requestContext.Handler.HandleAsync(requestContext).ContinueWith(HandleResponseAsync).Unwrap();

            Task HandleResponseAsync(Task<ResponseContext> x)
            {
                return x.IsCompletedSuccessfully ? HandleSuccess(x.Result) : HandleFault(x.Exception.InnerException);
            }

            Task HandleSuccess(ResponseContext ctx) =>
                    WriteResponse(httpResponse, ctx);

            Task HandleFault(Exception e)
            {
                Logger.LogError(e, "An exception occured while handling request.");
                return WriteException(httpResponse, e);
            }
        }

        private static Task WriteResponse(HttpResponse response, ResponseContext responseContext)
        {
            response.StatusCode = (int)responseContext.StatusCode;
            response.ContentType = responseContext.ContentType;

            foreach (var (key, value) in responseContext.Headers)
            {
                response.Headers[key] = value;
            }

            return responseContext.Body == null
                           ? Task.CompletedTask
                           : response.WriteAsync(responseContext.Body);
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