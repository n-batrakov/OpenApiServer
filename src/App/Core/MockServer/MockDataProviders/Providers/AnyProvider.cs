using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Mapping;
using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Providers
{
    public class AnyProvider : IMockDataProvider
    {
        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            if (schema.GetSchemaType() != OpenApiSchemaType.Any)
            {
                return false;
            }

            writer.WriteStartObject();
            writer.WriteEndObject();
            return true;
        }
    }
}