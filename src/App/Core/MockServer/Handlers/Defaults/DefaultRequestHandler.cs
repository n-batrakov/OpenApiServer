using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OpenApiServer.Core.MockServer.Context.Internals;
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
        
        private IOpenApiExampleProvider ExampleProvider { get; }
        private IHttpClientFactory HttpClientFactory { get; }

        public DefaultRequestHandler(
                Options config,
                IRequestValidator requestValidator,
                IResponseValidator responseValidator,
                IOpenApiExampleProvider exampleProvider,
                IHttpClientFactory httpClientFactory)
        {
            Config = config;
            RequestValidator = requestValidator;
            ResponseValidator = responseValidator;
            ExampleProvider = exampleProvider;
            HttpClientFactory = httpClientFactory;
        }

        public async Task<ResponseContext> HandleAsync(RouteContext request)
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

        private Task<ResponseContext> GetResponseAsync(RouteContext request)
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

        private Task<ResponseContext> GetMockResponseAsync(RouteContext request)
        {
            var mockHandler = new MockHandler(ExampleProvider);
            return mockHandler.HandleAsync(request);
        }

        private Task<ResponseContext> GetProxyResponseAsync(RouteContext request)
        {
            var proxyOptions = new ProxyHandler.Options() { Host = Config.Proxy };
            var proxyHandler = new ProxyHandler(HttpClientFactory, proxyOptions);
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