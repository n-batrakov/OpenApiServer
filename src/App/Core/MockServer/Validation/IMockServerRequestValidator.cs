using ITExpert.OpenApi.Core.MockServer.Context.Types;

namespace ITExpert.OpenApi.Core.MockServer.Validation
{
    public interface IMockServerRequestValidator
    {
        RequestValidationStatus Validate(RequestContext context);
    }
}