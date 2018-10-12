using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.Validation
{
    public class ResponseValidator : IResponseValidator
    {
        public HttpValidationStatus Validate(ResponseContext response, RouteContext context)
        {
            // TODO: Implement response validator
            return HttpValidationStatus.Success();
        }
    }
}