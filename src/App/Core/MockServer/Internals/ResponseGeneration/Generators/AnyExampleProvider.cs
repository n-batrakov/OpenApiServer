using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    public class AnyExampleProvider : IOpenApiExampleProvider
    {
        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
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