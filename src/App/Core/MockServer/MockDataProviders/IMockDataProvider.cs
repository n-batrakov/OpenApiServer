using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace OpenApiServer.Core.MockServer.MockDataProviders
{
    public interface IMockDataProvider
    {
        bool TryWriteValue(IOpenApiWriter writer, JSchema schema);
    }
}