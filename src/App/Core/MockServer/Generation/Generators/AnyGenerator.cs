using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Server.Core.MockServer.Generation.Generators
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