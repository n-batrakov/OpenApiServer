using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    public interface IResponseValidator
    {
        RequestValidationStatus Validate(string response, OpenApiSchema schema);
    }

    public class NullResponseValidator : IResponseValidator
    {
        public RequestValidationStatus Validate(string response, OpenApiSchema schema)
        {
            return RequestValidationStatus.Success();
        }
    }
}