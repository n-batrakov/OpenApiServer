using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Internals;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Providers
{
    //TODO: Tuples
    //TODO: Uniqueness
    public class ArrayProvider : IMockDataProvider
    {
        private IReadOnlyCollection<IMockDataProvider> ExampleProviders { get; }

        public ArrayProvider(IReadOnlyCollection<IMockDataProvider> providers)
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