using System;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration
{
    public static class OpenApiSchemaExtensions
    {
        public static OpenApiSchemaType ConvertTypeToEnum(this OpenApiSchema schema)
        {
            if (schema.Type == null)
            {
                var isObject = schema.Properties != null;
                if (isObject)
                {
                    return OpenApiSchemaType.Object;
                }

                var isArray = schema.Items != null;
                if (isArray)
                {
                    return OpenApiSchemaType.Array;
                }

                throw new ArgumentOutOfRangeException(nameof(schema), "Unknown type.");
            }
            return Enum.Parse<OpenApiSchemaType>(schema.Type, ignoreCase: true);
        }
    }
}