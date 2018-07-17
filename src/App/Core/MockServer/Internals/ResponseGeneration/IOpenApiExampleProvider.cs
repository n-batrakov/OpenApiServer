using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public interface IOpenApiExampleProvider
    {
        bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema);
    }
}