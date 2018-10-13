using System;
using System.Collections.Generic;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Mapping;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.MockDataProviders.Internals;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Providers
{
    public class CombinedSchemaDataProvider : IMockDataProvider
    {
        private Random Random { get; }
        private IReadOnlyCollection<IMockDataProvider> Providers { get; }

        public CombinedSchemaDataProvider(IReadOnlyCollection<IMockDataProvider> providers, Random random)
        {
            Providers = providers;
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            if (schema.GetSchemaType() != OpenApiSchemaType.Combined)
            {
                return false;
            }

            var schemes = SelectSchemes(schema);
            var combined = CombineSchemes(schemes);
            Providers.WriteValueOrThrow(writer, combined);

            return true;
        }

        private IEnumerable<JSchema> SelectSchemes(JSchema schema)
        {
            if (schema.AnyOf?.Count > 0)
            {
                yield return PickRandom(schema.AnyOf);
            }

            if (schema.OneOf?.Count > 0)
            {
                yield return PickRandom(schema.OneOf);
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


        private JSchema PickRandom(IList<JSchema> source)
        {
            var idx = Random.Next(0, source.Count);
            return source[idx];
        }
    }
}