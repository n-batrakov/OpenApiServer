using System;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Mapping;
using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Internals
{
    internal static class OpenApiSchemaTypesExtensions
    {
        public static bool IsString(this JSchema schema) => schema.GetSchemaType() == OpenApiSchemaType.String;
        public static bool IsObject(this JSchema schema) => schema.GetSchemaType() == OpenApiSchemaType.Object;
        public static bool IsArray(this JSchema schema) => schema.GetSchemaType() == OpenApiSchemaType.Array;

        public static bool IsFormattedString(this JSchema schema, string expectedFormat) =>
                schema.IsString() &&
                schema.Format != null &&
                schema.Format.Equals(expectedFormat, StringComparison.OrdinalIgnoreCase);
    }
}