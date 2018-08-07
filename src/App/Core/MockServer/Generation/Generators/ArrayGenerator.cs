using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Core.MockServer.Generation.Generators
{
    //TODO: Tuples
    //TODO: Uniqueness
    public class ArrayGenerator : IOpenApiExampleProvider
    {
        private IReadOnlyCollection<IOpenApiExampleProvider> ExampleProviders { get; }

        public ArrayGenerator(IReadOnlyCollection<IOpenApiExampleProvider> providers)
        {
            ExampleProviders = providers;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
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

        private void WriteItems(IOpenApiWriter writer, JSchema schema)
        {
            var minItems = schema.MinimumItems ?? 1;
            for (var i = 0; i < minItems; i++)
            {
                ExampleProviders.WriteValueOrThrow(writer, schema.Items.Single());
            }
        }
    }
}