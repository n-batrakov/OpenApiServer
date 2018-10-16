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
using OpenApiServer.Core.MockServer.Context.Internals;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Server.Logging;

using RouteContext = OpenApiServer.Core.MockServer.Context.Types.RouteContext;

namespace OpenApiServer.Core.MockServer
{
    public class MockServerBuilder
    {
        private IApplicationBuilder ApplicationBuilder { get; }

        private RouteContextProvider ContextProvider { get; }

        private ILogger Logger { get; }

        public MockServerBuilder(IApplicationBuilder app, IEnumerable<OpenApiDocument> specs)
        {
            ApplicationBuilder = app;
            ContextProvider = ActivatorUtilities.CreateInstance<RouteContextProvider>(app.ApplicationServices, specs);
            
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

        private Task HandleRequest(RouteContext requestContext, HttpResponse httpResponse)
        {
            return requestContext.Handler.HandleAsync(requestContext).ContinueWith(HandleResponseAsync).Unwrap();

            Task HandleResponseAsync(Task<ResponseContext> x)
            {
                if (x.IsCompletedSuccessfully)
                {
                    return x.Result == null ? HandleNullResponse(httpResponse) : HandleResponse(httpResponse, x.Result);
                }

                var exception = x.Exception.InnerException;
                
                return HandleException(httpResponse, exception);
            }
        }

        private static Task HandleResponse(HttpResponse response, ResponseContext responseContext)
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

        private Task HandleNullResponse(HttpResponse response)
        {
            Logger.LogWarning("No handler processed the request");

            response.StatusCode = 500;
            response.ContentType = "text/plain";
            var msg = "[NotProcessed] No handler processed the request. Check routes' `handler` option in the config.";
            return response.WriteAsync(msg, Encoding.UTF8);
        }

        private Task HandleException(HttpResponse response, Exception exception)
        {
            Logger.LogError(exception, "An exception occured while handling request.");

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