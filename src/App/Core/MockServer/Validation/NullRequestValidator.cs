using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public class NullRequestValidator : IMockServerRequestValidator
    {
        public RequestValidationStatus Validate(HttpRequestValidationContext context, OpenApiOperation operation) =>
                RequestValidationStatus.Success();
    }
}