using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Validation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Newtonsoft.Json;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockServerBuilder
    {
        private MockServerRequestContextProvider ContextProvider { get; }

        private IMockServerRequestValidator RequestValidator { get; }
        private IMockServerResponseValidator ResponseValidator { get; }

        private IMockServerRequestHandler RequestHandler { get; }

        public MockServerBuilder(MockServerRequestContextProvider contextProvider,
                                 IMockServerRequestValidator requestValidator,
                                 IMockServerResponseValidator responseValidator,
                                 IMockServerRequestHandler requestHandler)
        {
            ContextProvider = contextProvider;
            RequestValidator = requestValidator;
            ResponseValidator = responseValidator;
            RequestHandler = requestHandler;
        }

        public IRouter BuildRouter(IRouteBuilder builder)
        {
            foreach (var id in ContextProvider.Routes)
            {
                builder.MapVerb(id.Verb.ToString(), id.Path, HandleRequest);
            }

            return builder.Build();
        }

        private async Task HandleRequest(HttpContext ctx)
        {
            var requestContext = ContextProvider.GetContext(ctx);
            var config = requestContext.Options;
            ctx.Features.Set(requestContext);

            if (config.ShouldValidateRequest)
            {
                var requestValidationStatus = RequestValidator.Validate(requestContext);
                if (requestValidationStatus.IsFaulty)
                {
                    await RespondWithValidationErrorAsync(ctx.Response,
                                                          HttpStatusCode.BadRequest,
                                                          requestValidationStatus.Errors);
                    return;
                }
            }

            var responseContext = await RequestHandler.HandleAsync(requestContext);

            if (config.ShouldValidateResponse)
            {
                var responseValidationStatus = ResponseValidator.Validate(
                        responseContext,
                        requestContext.OperationSpec.Responses);

                if (responseValidationStatus.IsFaulty)
                {
                    await RespondWithValidationErrorAsync(ctx.Response,
                                                          HttpStatusCode.InternalServerError,
                                                          responseValidationStatus.Errors);
                    return;
                }

            }

            WriteResponse(ctx.Response, responseContext);
        }

        private static Task RespondWithValidationErrorAsync(HttpResponse response,
                                                            HttpStatusCode status,
                                                            IEnumerable<RequestValidationError> errors)
        {
            response.StatusCode = (int)status;
            response.ContentType = "application/json";
            var json = JsonConvert.SerializeObject(errors);
            return response.WriteAsync(json, Encoding.UTF8);
        }

        private static void WriteResponse(HttpResponse response, IMockServerResponseContext responseContext)
        {
            response.StatusCode = (int)responseContext.StatusCode;
            response.ContentType = responseContext.ContentType;
            response.Body = responseContext.Body;

            foreach (var header in responseContext.Headers)
            {
                response.Headers.Add(header);
            }
        }
    }
}