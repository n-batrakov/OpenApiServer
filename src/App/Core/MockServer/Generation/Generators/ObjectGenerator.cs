using System.Collections.Generic;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Generation.Generators
{
    //TODO: minProperties, maxProperties
    //TODO: patternProperties
    //TODO: Property dependencies
    public class ObjectGenerator : IOpenApiExampleProvider
    {
        private IReadOnlyCollection<IOpenApiExampleProvider> Providers { get; }
        private ObjectDepthCounter DepthCounter { get; }

        private static readonly string[] AdditionalPropertiesExampleNames =
        {
                "dynamicProp1",
                "dynamicProp2",
                "dynamicProp3"
        };

        public ObjectGenerator(IReadOnlyCollection<IOpenApiExampleProvider> providers, ObjectDepthCounter counter)
        {
            Providers = providers;
            DepthCounter = counter;
        }

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!schema.IsObject())
            {
                return false;
            }

            writer.WriteStartObject();

            using (DepthCounter.Enter())
            {
                if (DepthCounter.CanEnter)
                {
                    WriteProperties(writer, schema);
                }
            }

            writer.WriteEndObject();

            return true;
        }

        private void WriteProperties(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    writer.WritePropertyName(property.Key);
                    Providers.WriteValueOrThrow(writer, property.Value);
                }
            }

            if (schema.AdditionalProperties != null)
            {
                foreach (string property in AdditionalPropertiesExampleNames)
                {
                    writer.WritePropertyName(property);
                    Providers.WriteValueOrThrow(writer, schema.AdditionalProperties);
                }
            }
        }
    }
}