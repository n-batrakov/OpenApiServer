using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public interface IMockServerRequestValidator
    {
        RequestValidationStatus Validate(HttpRequestValidationContext context, OpenApiOperation operation);
    }
}