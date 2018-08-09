using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace OpenApiServer.Core.MockServer.Generation
{
    public interface IOpenApiExampleProvider
    {
        bool TryWriteValue(IOpenApiWriter writer, JSchema schema);
    }
}