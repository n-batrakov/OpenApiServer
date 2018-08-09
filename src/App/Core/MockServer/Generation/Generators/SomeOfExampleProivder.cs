using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Generation.Internals;
using OpenApiServer.Core.MockServer.Generation.Types;

namespace OpenApiServer.Core.MockServer.Generation.Generators
{
    public class CombinedGenerator : IOpenApiExampleProvider
    {
        private IReadOnlyCollection<IOpenApiExampleProvider> Providers { get; }

        public CombinedGenerator(IReadOnlyCollection<IOpenApiExampleProvider> providers)
        {
            Providers = providers;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
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

        private static IEnumerable<JSchema> SelectSchemes(JSchema schema)
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

        private static JSchema CombineSchemes(IEnumerable<JSchema> schemes)
        {
            var result = new JSchema();
            foreach (var schema in schemes)
            {
                result.Properties.AddRange(schema.Properties);
                result.ExtensionData.AddRange(schema.ExtensionData);

                result.Enum.AddRange(schema.Enum);
                result.AnyOf.AddRange(schema.AnyOf);
                result.AllOf.AddRange(schema.AllOf);
                result.OneOf.AddRange(schema.OneOf);

                result.Items.Clear();
                result.Items.AddRange(schema.Items);

                result.AdditionalProperties = schema.AdditionalProperties;
                result.Not = schema.Not;

                result.UniqueItems = schema.UniqueItems;
                result.AllowAdditionalProperties = schema.AllowAdditionalProperties;
                result.Type = schema.Type;
                result.Format = schema.Format;
                result.Pattern = schema.Pattern;
                result.Maximum = schema.Maximum;
                result.Minimum = schema.Minimum;
                result.ExclusiveMaximum = schema.ExclusiveMaximum;
                result.ExclusiveMinimum = schema.ExclusiveMinimum;
                result.MaximumItems = schema.MaximumItems;
                result.MinimumItems = schema.MinimumItems;
                result.MaximumProperties = schema.MaximumProperties;
                result.MinimumProperties = schema.MinimumProperties;
                result.MaximumLength = schema.MaximumLength;
                result.MinimumLength = schema.MinimumLength;
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