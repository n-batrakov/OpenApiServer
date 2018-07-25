using ITExpert.OpenApi.Server.Core.MockServer.Types;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public interface IMockServerRequestValidator
    {
        RequestValidationStatus Validate(IMockServerRequestContext context);
    }
}