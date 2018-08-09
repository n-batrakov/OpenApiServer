using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Validation.Types;

namespace OpenApiServer.Core.MockServer.Validation
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