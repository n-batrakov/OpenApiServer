using System;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Generation.Types;

namespace OpenApiServer.Core.MockServer.Generation.Internals
{
    internal static class OpenApiSchemaTypesExtensions
    {
        public static bool IsString(this JSchema schema) => schema.ConvertTypeToEnum() == OpenApiSchemaType.String;
        public static bool IsObject(this JSchema schema) => schema.ConvertTypeToEnum() == OpenApiSchemaType.Object;
        public static bool IsArray(this JSchema schema) => schema.ConvertTypeToEnum() == OpenApiSchemaType.Array;

        public static bool IsFormattedString(this JSchema schema, string expectedFormat) =>
                schema.IsString() &&
                schema.Format != null &&
                schema.Format.Equals(expectedFormat, StringComparison.OrdinalIgnoreCase);
    }
}