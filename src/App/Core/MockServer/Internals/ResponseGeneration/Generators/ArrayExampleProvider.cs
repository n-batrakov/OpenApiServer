using System.Collections.Generic;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    //TODO: Tuples
    //TODO: Uniqueness
    public class ArrayExampleProvider : IOpenApiExampleProvider
    {
        private IReadOnlyCollection<IOpenApiExampleProvider> ExampleProviders { get; }

        public ArrayExampleProvider(IReadOnlyCollection<IOpenApiExampleProvider> providers)
        {
            ExampleProviders = providers;
        }

        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!schema.IsArray())
            {
                return false;
            }

            writer.WriteStartArray();
            WriteItems(writer, schema);
            writer.WriteEndArray();

            return true;
        }

        private void WriteItems(IOpenApiWriter writer, OpenApiSchema schema)
        {
            var minItems = schema.MinItems ?? 1;
            for (var i = 0; i < minItems; i++)
            {
                ExampleProviders.WriteValueOrThrow(writer, schema.Items);
            }
        }
    }
}