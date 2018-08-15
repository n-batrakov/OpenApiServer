using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("validateResponse")]
    public class ValidateResponseHandler : IRequestHandler
    {
        private IResponseValidator ResponseValidator { get; }
        private ResponseContext Response { get; }

        public ValidateResponseHandler(IResponseValidator responseValidator, ResponseContext response)
        {
            ResponseValidator = responseValidator;
            Response = response;
        }

        public Task<ResponseContext> HandleAsync(RequestContext context)
        {
            var responseValidationStatus = ResponseValidator.Validate(Response, context);

            return responseValidationStatus.IsSuccess
                           ? Task.FromResult((ResponseContext)null)
                           : Task.FromResult(Error(responseValidationStatus));
        }

        private static ResponseContext Error(RequestValidationStatus validationStatus) =>
                new ResponseContext
                {
                        BreakPipeline = true,
                        StatusCode = HttpStatusCode.InternalServerError,
                        ContentType = "application/json",
                        Body = JsonConvert.SerializeObject(validationStatus)
                };
    }
}