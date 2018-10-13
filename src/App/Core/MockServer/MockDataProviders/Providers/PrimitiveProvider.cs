using System;
using System.Globalization;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Mapping;
using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Providers
{
    public class PrimitiveProvider : IMockDataProvider
    {
        private Random Random { get; }

        public PrimitiveProvider(Random random)
        {
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            switch (schema.GetSchemaType())
            {
                case OpenApiSchemaType.Null:
                    writer.WriteNull();
                    return true;
                case OpenApiSchemaType.Boolean:
                    writer.WriteValue(true);
                    return true;
                case OpenApiSchemaType.Integer:
                    writer.WriteValue(GetIntValue(schema));
                    return true;
                case OpenApiSchemaType.Number:
                    var rawValue = GetNumberValue(schema).ToString("F", CultureInfo.InvariantCulture);
                    writer.WriteRaw(rawValue);
                    return true;
                case OpenApiSchemaType.String:
                case OpenApiSchemaType.Object:
                case OpenApiSchemaType.Array:
                case OpenApiSchemaType.Any:
                case OpenApiSchemaType.Combined:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int GetIntValue(JSchema schema)
        {
            //TODO: MultipleOf
            var min = (int)(schema.Minimum ?? 0);
            var max = (int)(schema.Maximum ?? 100);
            return Random.Next(min, max);
        }

        private double GetNumberValue(JSchema schema)
        {
            //TODO: MultipleOf
            var min = schema.Minimum ?? 0;
            var max = schema.Maximum ?? 100;
            var number = Random.NextDouble();
            return min + (max - min) * number;
        }
    }
}