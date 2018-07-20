using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Generation.Generators
{
    public class CombinedGenerator : IOpenApiExampleProvider
    {
        private IReadOnlyCollection<IOpenApiExampleProvider> Providers { get; }

        public CombinedGenerator(IReadOnlyCollection<IOpenApiExampleProvider> providers)
        {
            Providers = providers;
        }

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.ConvertTypeToEnum() != OpenApiSchemaType.Combined)
            {
                return false;
            }

            var schemes = SelectSchemes(schema);
            var combined = CombineSchemes(schemes);
            Providers.WriteValueOrThrow(writer, combined);

            return true;
        }

        private static IEnumerable<OpenApiSchema> SelectSchemes(OpenApiSchema schema)
        {
            if (schema.AnyOf?.Count > 0)
            {
                yield return schema.AnyOf.TakeRandom().First();
            }

            if (schema.OneOf?.Count > 0)
            {
                yield return schema.OneOf.TakeRandom().First();
            }

            if (schema.AllOf?.Count > 0)
            {
                foreach (var allOf in schema.AllOf)
                {
                    yield return allOf;
                }
            }
        }

        private static OpenApiSchema CombineSchemes(IEnumerable<OpenApiSchema> schemes)
        {
            var result = new OpenApiSchema();
            foreach (var schema in schemes)
            {
                result.Properties.AddRange(schema.Properties);
                result.Extensions.AddRange(schema.Extensions);

                result.Enum.AddRange(schema.Enum);
                result.AnyOf.AddRange(schema.AnyOf);
                result.AllOf.AddRange(schema.AllOf);
                result.OneOf.AddRange(schema.OneOf);


                result.Items = schema.Items;
                result.AdditionalProperties = schema.AdditionalProperties;
                result.Not = schema.Not;

                result.UniqueItems = schema.UniqueItems;
                result.AdditionalPropertiesAllowed = schema.AdditionalPropertiesAllowed;
                result.Example = schema.Example;
                result.Nullable = schema.Nullable;
                result.Discriminator = schema.Discriminator;
                result.Type = schema.Type;
                result.Format = schema.Format;
                result.Pattern = schema.Pattern;
                result.Maximum = schema.Maximum;
                result.Minimum = schema.Minimum;
                result.ExclusiveMaximum = schema.ExclusiveMaximum;
                result.ExclusiveMinimum = schema.ExclusiveMinimum;
                result.MaxItems = schema.MaxItems;
                result.MinItems = schema.MinItems;
                result.MaxProperties = schema.MaxProperties;
                result.MinProperties = schema.MinProperties;
                result.MaxLength = schema.MaxLength;
                result.MinLength = schema.MinLength;
                result.MultipleOf = schema.MultipleOf;
                result.Default = schema.Default;
                result.ReadOnly = schema.ReadOnly;
                result.WriteOnly = schema.WriteOnly;

                //result.Required = schema.Required;
                //result.Reference = schema.Reference;
                //result.Title = schema.Title;
                //result.Description = schema.Description;
                //result.UnresolvedReference = schema.UnresolvedReference;
                //result.ExternalDocs = schema.ExternalDocs;
                //result.Xml = schema.Xml;
                //result.Deprecated = schema.Deprecated;
            }

            return result;
        }
    }
}