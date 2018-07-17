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
                var isObject = schema.Properties != null && schema.Properties.Count > 0;
                if (isObject)
                {
                    return OpenApiSchemaType.Object;
                }

                var isArray = schema.Items != null;
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
            return Enum.Parse<OpenApiSchemaType>(schema.Type, ignoreCase: true);
        }
    }
}