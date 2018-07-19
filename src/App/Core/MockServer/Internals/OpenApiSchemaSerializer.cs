using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals
{
    public static class OpenApiSchemaSerializer
    {
        public static string ToJson(this OpenApiSchema schema)
        {
            return OpenApiSerializer.Serialize(x => WriteSchema(x, schema));
        }

        private static void WriteSchema(IOpenApiWriter writer, OpenApiSchema schema)
        {
            schema.SerializeAsV3WithoutReference(writer);
        }
    }
}