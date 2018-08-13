using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.ExampleProviders.Internals;

namespace OpenApiServer.Core.MockServer.ExampleProviders.Providers
{
    //TODO: Tuples
    //TODO: Uniqueness
    public class ArrayProvider : IOpenApiExampleProvider
    {
        private IReadOnlyCollection<IOpenApiExampleProvider> ExampleProviders { get; }

        public ArrayProvider(IReadOnlyCollection<IOpenApiExampleProvider> providers)
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