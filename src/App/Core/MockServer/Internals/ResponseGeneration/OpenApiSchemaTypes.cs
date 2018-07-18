using System;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration
{
    internal static class OpenApiSchemaTypes
    {
        public static bool IsString(this OpenApiSchema schema) => schema.ConvertTypeToEnum() == OpenApiSchemaType.String;
        public static bool IsObject(this OpenApiSchema schema) => schema.ConvertTypeToEnum() == OpenApiSchemaType.Object;
        public static bool IsArray(this OpenApiSchema schema) => schema.ConvertTypeToEnum() == OpenApiSchemaType.Array;

        public static bool IsFormattedString(this OpenApiSchema schema, string expectedFormat) =>
                schema.IsString() &&
                schema.Format != null &&
                schema.Format.Equals(expectedFormat, StringComparison.OrdinalIgnoreCase);
    }
}