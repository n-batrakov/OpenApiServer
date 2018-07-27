using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public interface IMockServerRequestValidator
    {
        RequestValidationStatus Validate(RequestContext context);
    }
}