using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Generation.Generators
{
    public class Base64Generator : IOpenApiExampleProvider
    {
        private const string Base64 = "TW9jayBzZXJ2ZXIgZ2VuZXJhdGVkIGZpbGU=";

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!schema.IsFormattedString("base64"))
            {
                return false;
            }

            writer.WriteValue(Base64);

            return true;
        }
    }
}