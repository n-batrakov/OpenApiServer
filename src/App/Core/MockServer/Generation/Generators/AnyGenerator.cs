using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Generation.Internals;
using OpenApiServer.Core.MockServer.Generation.Types;

namespace OpenApiServer.Core.MockServer.Generation.Generators
{
    public class AnyGenerator : IOpenApiExampleProvider
    {
        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            if (schema.ConvertTypeToEnum() != OpenApiSchemaType.Any)
            {
                return false;
            }

            writer.WriteStartObject();
            writer.WriteEndObject();
            return true;
        }
    }
}