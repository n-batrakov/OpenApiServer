using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public interface IMockServerResponseValidator
    {
        RequestValidationStatus Validate(string response, OpenApiSchema schema);
    }

    public class NullResponseValidator : IMockServerResponseValidator
    {
        public RequestValidationStatus Validate(string response, OpenApiSchema schema)
        {
            return RequestValidationStatus.Success();
        }
    }
}