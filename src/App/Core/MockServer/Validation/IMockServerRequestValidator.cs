using ITExpert.OpenApi.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Core.MockServer.Validation.Types;

namespace ITExpert.OpenApi.Core.MockServer.Validation
{
    public interface IMockServerRequestValidator
    {
        RequestValidationStatus Validate(RequestContext context);
    }
}