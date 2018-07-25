using ITExpert.OpenApi.Server.Core.MockServer.Types;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public interface IMockServerResponseValidator
    {
        RequestValidationStatus Validate(IMockServerResponseContext context, OpenApiResponses responsesSpec);
    }

    public class NullResponseValidator : IMockServerResponseValidator
    {
        public RequestValidationStatus Validate(IMockServerResponseContext context, OpenApiResponses responsesSpec)
        {
            return RequestValidationStatus.Success();
        }
    }
}