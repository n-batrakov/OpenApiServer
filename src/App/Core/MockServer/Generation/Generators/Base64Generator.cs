using ITExpert.OpenApi.Server.Core.MockServer.Generation.Internals;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Server.Core.MockServer.Generation.Generators
{
    public class Base64Generator : IOpenApiExampleProvider
    {
        private const string Base64 = "TW9jayBzZXJ2ZXIgZ2VuZXJhdGVkIGZpbGU=";

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
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