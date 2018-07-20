using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Generation;
using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    internal class OpenApiSchemaConverter
    {
        private Dictionary<OpenApiSchema, JSchema> ReferenceMap { get; }

        public OpenApiSchemaConverter()
        {
            ReferenceMap = new Dictionary<OpenApiSchema, JSchema>();
        }

        public JSchema Convert(OpenApiSchema oasSchema)
        {
            if (oasSchema == null)
            {
                return null;
            }
            if (ReferenceMap.TryGetValue(oasSchema, out var reference))
            {
                return reference;
            }

            var jSchema =
                    new JSchema
                    {
                        Type = ConvertType(oasSchema.Type),
                        Format = oasSchema.Format,
                        AdditionalProperties = Convert(oasSchema.AdditionalProperties),
                        AllowAdditionalProperties = oasSchema.AdditionalPropertiesAllowed,
                        Default = ConvertAny(oasSchema.Default),
                        UniqueItems = oasSchema.UniqueItems ?? false,
                        ExclusiveMaximum = oasSchema.ExclusiveMaximum ?? true,
                        ExclusiveMinimum = oasSchema.ExclusiveMinimum ?? false,
                        MultipleOf = (double?)oasSchema.MultipleOf,
                        Maximum = (double?)oasSchema.Maximum,
                        Minimum = (double?)oasSchema.Minimum,
                        MaximumItems = oasSchema.MaxItems,
                        MinimumItems = oasSchema.MinItems,
                        MaximumLength = oasSchema.MaxLength,
                        MinimumLength = oasSchema.MinLength,
                        MaximumProperties = oasSchema.MaxProperties,
                        MinimumProperties = oasSchema.MinProperties,
                        Not = Convert(oasSchema.Not),
                        Pattern = oasSchema.Pattern,
                    };
            FillCollections(jSchema, oasSchema);
            ReferenceMap.Add(oasSchema, jSchema);
            return jSchema;
        }

        private void FillCollections(JSchema jSchema, OpenApiSchema spec)
        {
            jSchema.AllOf.AddRange(Convert(spec.AllOf));
            jSchema.OneOf.AddRange(Convert(spec.OneOf));
            jSchema.AnyOf.AddRange(Convert(spec.AnyOf));

            jSchema.Required.AddRange(spec.Required);
            jSchema.Dependencies.AddRange(jSchema.Dependencies);
            jSchema.Enum.AddRange(spec.Enum.Select(ConvertAny));

            var items = Convert(spec.Items);
            if (items != null)
            {
                jSchema.Items.Add(items);
            }

            jSchema.Properties.AddRange(spec.Properties.ToDictionary(x => x.Key, x => Convert(x.Value)));
        }

        private IEnumerable<JSchema> Convert(IEnumerable<OpenApiSchema> schema)
        {
            return schema.Select(Convert).Where(x => x != null);
        }

        private static JSchemaType? ConvertType(string type)
        {
            var isParsed = Enum.TryParse<JSchemaType>(type, ignoreCase: true, out var result);
            if (isParsed)
            {
                return result;
            }

            return null;
        }

        private static JToken ConvertAny(IOpenApiAny any)
        {
            if (any == null)
            {
                return null;
            }
            var json = OpenApiSerializer.Serialize(any.Write);
            return JToken.Parse(json);
        }
    }
}