using ITExpert.OpenApi.Server.Core.MockServer.Types;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public class NullRequestValidator : IMockServerRequestValidator
    {
        public RequestValidationStatus Validate(IMockServerRequestContext context)
        {
            return RequestValidationStatus.Success();
        }
    }
}