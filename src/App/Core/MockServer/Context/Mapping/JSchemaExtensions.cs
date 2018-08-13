using System;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Context.Mapping
{
    public static class JSchemaExtensions
    {
        private const string ExampleKey = "x-example";

        public static void SetExample(this JSchema schema, JToken token)
        {
            schema.ExtensionData[ExampleKey] = token;
        }

        public static JToken GetExample(this JSchema schema)
        {
            schema.ExtensionData.TryGetValue(ExampleKey, out var example);
            return example;
        }

        public static OpenApiSchemaType GetSchemaType(this JSchema schema)
        {
            if (schema.Type == null)
            {
                var isObject = schema.Properties != null && schema.Properties.Count > 0;
                if (isObject)
                {
                    return OpenApiSchemaType.Object;
                }

                var isArray = schema.Items.Count > 0;
                if (isArray)
                {
                    return OpenApiSchemaType.Array;
                }

                if (schema.OneOf?.Count > 0 || schema.AllOf?.Count > 0 || schema.AnyOf?.Count > 0)
                {
                    return OpenApiSchemaType.Combined;
                }

                return OpenApiSchemaType.Any;
            }

            return Enum.Parse<OpenApiSchemaType>(schema.Type.ToString(), ignoreCase: true);
        }
    }
}