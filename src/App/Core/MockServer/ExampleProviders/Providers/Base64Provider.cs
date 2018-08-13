using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.ExampleProviders.Internals;

namespace OpenApiServer.Core.MockServer.ExampleProviders.Providers
{
    public class Base64Provider : IOpenApiExampleProvider
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