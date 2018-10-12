using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.Validation
{
    public interface IResponseValidator
    {
        HttpValidationStatus Validate(ResponseContext response, RouteContext context);
    }
}