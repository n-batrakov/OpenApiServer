using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation;
using OpenApiServer.Core.MockServer.Validation.Types;
using OpenApiServer.Utils;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("validateRequest")]
    public class ValidateRequestHandler : IRequestHandler
    {
        private IRequestValidator RequestValidator { get; }

        public ValidateRequestHandler(IRequestValidator requestValidator)
        {
            RequestValidator = requestValidator;
        }

        public Task<ResponseContext> HandleAsync(RouteContext request)
        {
            var requestValidationStatus = RequestValidator.Validate(request);

            return requestValidationStatus.IsSuccess
                           ? Task.FromResult((ResponseContext)null)
                           : Task.FromResult(Error(requestValidationStatus));
        }

        private static ResponseContext Error(
                HttpValidationStatus httpValidationStatus) =>
                new ResponseContext
                {
                        BreakPipeline = true,
                        StatusCode = HttpStatusCode.BadRequest,
                        ContentType = "application/json",
                        Body = JsonConvert.SerializeObject(httpValidationStatus, JsonSettings.Value)
                };
    }
}