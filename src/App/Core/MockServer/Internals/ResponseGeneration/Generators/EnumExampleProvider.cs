using System;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    public class EnumExampleProvider : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public EnumExampleProvider(Random random)
        {
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            var isEnum = schema.Enum != null && schema.Enum.Count > 0;
            if (!isEnum)
            {
                return false;
            }

            var valueIndex = Random.Next(0, schema.Enum.Count);
            var value = schema.Enum[valueIndex];
            value.Write(writer);

            return true;
        }
    }
}