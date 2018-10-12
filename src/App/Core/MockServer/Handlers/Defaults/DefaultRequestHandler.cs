using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OpenApiServer.Core.MockServer.Context;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.ExampleProviders;
using OpenApiServer.Core.MockServer.Validation;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("default", typeof(Options))]
    public class DefaultRequestHandler : IRequestHandler
    {
        private Options Config { get; }
        private IRequestValidator RequestValidator { get; }
        private IResponseValidator ResponseValidator { get; }
        
        private RequestHandlerFactory HandlerFactory { get; }

        private IOpenApiExampleProvider ExampleProvider { get; }
        private IHttpClientFactory HttpClientFactory { get; }

        public DefaultRequestHandler(
            Options config, 
            IRequestValidator requestValidator, 
            IResponseValidator responseValidator, 
            RequestHandlerFactory handlerFactory, 
            IOpenApiExampleProvider exampleProvider, 
            IHttpClientFactory httpClientFactory)
        {
            this.Config = config;
            this.RequestValidator = requestValidator;
            this.ResponseValidator = responseValidator;
            this.HandlerFactory = handlerFactory;
            this.ExampleProvider = exampleProvider;
            this.HttpClientFactory = httpClientFactory;
        }

        public async Task<ResponseContext> HandleAsync(RequestContext request)
        {
            if (Config.ValidateRequest)
            {
                var requestValidationStatus = RequestValidator.Validate(request);
                if (!requestValidationStatus.IsSuccess)
                {
                    return ValidationError(requestValidationStatus);
                }
            }

            var responseTask = GetResponseAsync(request);
            var delayTask = Task.Delay(Config.Delay);
            await Task.WhenAll(responseTask, delayTask);
            var response = responseTask.Result;

            if (Config.ValidateResponse)
            {
                var responseValidationStatus = ResponseValidator.Validate(response, request);
                if (!responseValidationStatus.IsSuccess)
                {
                    return ValidationError(responseValidationStatus);
                }
            }

            return response;
        }

        private Task<ResponseContext> GetResponseAsync(RequestContext request)
        {
            switch (Config.Mock)
            {
                case MockMode.Default:
                case MockMode.None:
                    return GetProxyResponseAsync(request);
                case MockMode.Merge:
                    var mockTask = GetMockResponseAsync(request);
                    var proxyTask = GetProxyResponseAsync(request);
                    return Task
                        .WhenAll(mockTask, proxyTask)
                        .ContinueWith(_ => mockTask.Result.Merge(proxyTask.Result));
                case MockMode.Replace:
                    return GetMockResponseAsync(request);
                default:
                    throw new ArgumentOutOfRangeException(nameof(request));
            }
        }

        private Task<ResponseContext> GetMockResponseAsync(RequestContext request)
        {
            var mockHandler = new MockRequestHandler(ExampleProvider);
            return mockHandler.HandleAsync(request);
        }

        private Task<ResponseContext> GetProxyResponseAsync(RequestContext request)
        {
            var proxyOptions = new ProxyRequestHandler.Options() { Host = Config.Proxy };
            var proxyHandler = new ProxyRequestHandler(HttpClientFactory, proxyOptions);
            return proxyHandler.HandleAsync(request);
        }

        private static ResponseContext ValidationError(HttpValidationStatus httpValidationStatus) =>
                new ResponseContext
                {
                        BreakPipeline = true,
                        StatusCode = HttpStatusCode.BadRequest,
                        ContentType = "application/json",
                        Body = JsonConvert.SerializeObject(httpValidationStatus)
                };



        public class Options
        {
            public ValidationMode Validate { get; set; }
            public int Delay { get; set; }
            public string Proxy { get; set; }
            public MockMode Mock { get; set; }

            public bool ValidateRequest => Validate == ValidationMode.All || Validate == ValidationMode.Request;
            public bool ValidateResponse => Validate == ValidationMode.All || Validate == ValidationMode.Response;
        }

        public enum ValidationMode
        {
            None,
            Request,
            Response,
            All
        }

        public enum MockMode
        {
            Default,
            None,
            Merge,
            Replace
        }
    }
}