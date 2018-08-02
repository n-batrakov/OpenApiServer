using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Server.Core.MockServer.Validation.Types;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
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