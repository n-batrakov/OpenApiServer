using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.Validation
{
    public interface IRequestValidator
    {
        RequestValidationStatus Validate(RequestContext context);
    }
}