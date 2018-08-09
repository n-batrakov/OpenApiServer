using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Generation.Internals;

namespace OpenApiServer.Core.MockServer.Generation.Generators
{
    public class EnumGenerator : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public EnumGenerator(Random random)
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