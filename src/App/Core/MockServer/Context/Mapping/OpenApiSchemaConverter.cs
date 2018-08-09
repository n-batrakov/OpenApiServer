using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Generation.Internals;
using OpenApiServer.Utils;

namespace OpenApiServer.Core.MockServer.Context.Mapping
{
    public class OpenApiSchemaConverter
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

            var jSchema = new JSchema();
            ReferenceMap.Add(oasSchema, jSchema);

            jSchema.Type = ConvertType(oasSchema.Type);
            jSchema.Format = oasSchema.Format;
            jSchema.AdditionalProperties = Convert(oasSchema.AdditionalProperties);
            jSchema.AllowAdditionalProperties = oasSchema.AdditionalPropertiesAllowed;
            jSchema.Default = ConvertAny(oasSchema.Default);
            jSchema.UniqueItems = oasSchema.UniqueItems ?? false;
            jSchema.ExclusiveMaximum = oasSchema.ExclusiveMaximum ?? true;
            jSchema.ExclusiveMinimum = oasSchema.ExclusiveMinimum ?? false;
            jSchema.MultipleOf = (double?)oasSchema.MultipleOf;
            jSchema.Maximum = (double?)oasSchema.Maximum;
            jSchema.Minimum = (double?)oasSchema.Minimum;
            jSchema.MaximumItems = oasSchema.MaxItems;
            jSchema.MinimumItems = oasSchema.MinItems;
            jSchema.MaximumLength = oasSchema.MaxLength;
            jSchema.MinimumLength = oasSchema.MinLength;
            jSchema.MaximumProperties = oasSchema.MaxProperties;
            jSchema.MinimumProperties = oasSchema.MinProperties;
            jSchema.Not = Convert(oasSchema.Not);
            jSchema.Pattern = oasSchema.Pattern;

            jSchema.SetExample(ConvertAny(oasSchema.Example));
            FillCollections(jSchema, oasSchema);
            
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