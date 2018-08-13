using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.ExampleProviders.Internals;

namespace OpenApiServer.Core.MockServer.ExampleProviders.Providers
{
    public class EnumProvider : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public EnumProvider(Random random)
        {
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            var isEnum = schema.Enum != null && schema.Enum.Count > 0;
            if (!isEnum)
            {
                return false;
            }

            var valueIndex = Random.Next(0, schema.Enum.Count);
            var value = schema.Enum[valueIndex];
            writer.WriteJToken(value);

            return true;
        }
    }
}