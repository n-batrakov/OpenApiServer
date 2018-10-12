using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.Validation
{
    public interface IResponseValidator
    {
        HttpValidationStatus Validate(ResponseContext response, RequestContext context);
    }

    public class ResponseValidator : IResponseValidator
    {
        public HttpValidationStatus Validate(ResponseContext response, RequestContext context)
        {
            return HttpValidationStatus.Success();
        }
    }
}