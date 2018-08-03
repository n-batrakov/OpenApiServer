using ITExpert.OpenApi.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Core.MockServer.Validation.Types;

namespace ITExpert.OpenApi.Core.MockServer.Validation
{
    public interface IMockServerResponseValidator
    {
        RequestValidationStatus Validate(MockServerResponseContext response, RequestContext context);
    }

    public class MockServerResponseValidator : IMockServerResponseValidator
    {
        public RequestValidationStatus Validate(MockServerResponseContext response, RequestContext context)
        {
            return RequestValidationStatus.Success();
        }
    }
}