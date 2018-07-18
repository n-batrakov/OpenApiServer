using System;
using System.Collections.Generic;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    //TODO: minProperties, maxProperties
    //TODO: patternProperties
    //TODO: Property dependencies
    public class ObjectExampleProvider : IOpenApiExampleProvider
    {
        private Random Random { get; }
        private IReadOnlyCollection<IOpenApiExampleProvider> Providers { get; }

        private static readonly string[] AdditionalPropertiesExampleNames =
        {
                "DynamicProp1",
                "DynamicProp2",
                "DynamicProp3"
        };

        public ObjectExampleProvider(IReadOnlyCollection<IOpenApiExampleProvider> providers, Random random)
        {
            Providers = providers;
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!schema.IsObject())
            {
                return false;
            }

            writer.WriteStartObject();
            {
                WriteAllOfSchemas(writer, schema);
                WriteAnyOfSchema(writer, schema);
                WriteOneOfSchema(writer, schema);
                WriteProperties(writer, schema);
            }
            writer.WriteEndObject();

            return true;
        }

        private void WriteAllOfSchemas(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.AllOf == null || schema.AllOf.Count == 0)
            {
                return;
            }

            foreach (var allOfSchema in schema.AllOf)
            {
                WriteProperties(writer, allOfSchema);
            }
        }

        private void WriteAnyOfSchema(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.AnyOf == null || schema.AnyOf.Count == 0)
            {
                return;
            }

            foreach (var anyOfSchema in schema.AnyOf.TakeRandom())
            {
                WriteProperties(writer, anyOfSchema);
            }
        }

        private void WriteOneOfSchema(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.OneOf == null || schema.OneOf.Count == 0)
            {
                return;
            }

            var index = Random.Next(0, schema.OneOf.Count);
            var oneOfSchema = schema.OneOf[index];
            WriteProperties(writer, oneOfSchema);
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