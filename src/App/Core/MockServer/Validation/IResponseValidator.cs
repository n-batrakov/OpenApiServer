using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.Validation
{
    public interface IResponseValidator
    {
        RequestValidationStatus Validate(ResponseContext response, RequestContext context);
    }

    public class ResponseValidator : IResponseValidator
    {
        public RequestValidationStatus Validate(ResponseContext response, RequestContext context)
        {
            return RequestValidationStatus.Success();
        }
    }
}