using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace OpenApiServer.Core.MockServer.ExampleProviders
{
    public interface IOpenApiExampleProvider
    {
        bool TryWriteValue(IOpenApiWriter writer, JSchema schema);
    }
}