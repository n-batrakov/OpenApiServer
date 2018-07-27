using System.Net;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Server.Core.MockServer.Validation;

using Newtonsoft.Json;

namespace ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers
{
    public class MockServerRequestHandler : IMockServerRequestHandler
    {
        private IMockServerRequestValidator RequestValidator { get; }
        private IMockServerResponseValidator ResponseValidator { get; }

        private MockRequestHandler MockHandler { get; }
        private ProxyRequestHandler ProxyHandler { get; }

        public MockServerRequestHandler(IMockServerRequestValidator requestValidator,
                                        IMockServerResponseValidator responseValidator,
                                        MockRequestHandler mockHandler,
                                        ProxyRequestHandler proxyHandler)
        {
            RequestValidator = requestValidator;
            ResponseValidator = responseValidator;
            MockHandler = mockHandler;
            ProxyHandler = proxyHandler;
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
            var responseTask = context.Config.Mock
                                       ? MockHandler.HandleAsync(context)
                                       : ProxyHandler.HandleAsync(context);
            await Task.WhenAll(delayTask, responseTask);

            if (context.Config.ValidateResponse)
            {
                var responseValidationStatus = ResponseValidator.Validate(responseTask.Result, context);
                if (!responseValidationStatus.IsSuccess)
                {
                    return Error(responseValidationStatus,
                                                            HttpStatusCode.InternalServerError);
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