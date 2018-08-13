using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.RequestHandlers.Defaults
{
    public class ConfigurableRequestHandler : IMockServerRequestHandler
    {
        private IMockServerRequestValidator RequestValidator { get; }
        private IMockServerResponseValidator ResponseValidator { get; }
        private IMockServerRequestHandlerProvider HandlerProvider { get; }

        public ConfigurableRequestHandler(IMockServerRequestValidator requestValidator,
                                        IMockServerResponseValidator responseValidator,
                                        IMockServerRequestHandlerProvider handlerProvider)
        {
            RequestValidator = requestValidator;
            ResponseValidator = responseValidator;
            HandlerProvider = handlerProvider;
        }

        public async Task<MockServerResponseContext> HandleAsync(RequestContext context)
        {
            if (context.Config.ValidateRequest)
            {
                var requestValidationStatus = RequestValidator.Validate(context);
                if (!requestValidationStatus.IsSuccess)
                {
                    return Error(requestValidationStatus, HttpStatusCode.BadRequest);
                }
            }

            var delayTask = Task.Delay(context.Config.Delay);
            var responseTask = HandlerProvider.GetHandler(context.Config.Handler).HandleAsync(context);
            await Task.WhenAll(delayTask, responseTask);

            if (context.Config.ValidateResponse)
            {
                var responseValidationStatus = ResponseValidator.Validate(responseTask.Result, context);
                if (!responseValidationStatus.IsSuccess)
                {
                    return Error(responseValidationStatus, HttpStatusCode.InternalServerError);
                }

            }

            return responseTask.Result;
        }

        private static MockServerResponseContext Error(
                RequestValidationStatus validationStatus,
                HttpStatusCode statusCode)
        {
            return new MockServerResponseContext
                   {
                           StatusCode = statusCode,
                           ContentType = "application/json",
                           Body = JsonConvert.SerializeObject(validationStatus)
                   };
        }
    }
}