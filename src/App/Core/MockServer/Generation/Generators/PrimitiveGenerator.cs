using System;

using ITExpert.OpenApi.Server.Core.MockServer.Generation.Internals;
using ITExpert.OpenApi.Server.Core.MockServer.Generation.Types;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Server.Core.MockServer.Generation.Generators
{
    public class PrimitiveGenerator : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public PrimitiveGenerator(Random random)
        {
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            switch (schema.ConvertTypeToEnum())
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
                    writer.WriteValue(GetNumberValue(schema));
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
            var max = (int)(schema.Maximum ?? int.MaxValue);
            return Random.Next(min, max);
        }

        private double GetNumberValue(JSchema schema)
        {
            //TODO: MultipleOf
            var min = schema.Minimum ?? 0;
            var max = schema.Maximum ?? double.MaxValue;
            var number = Random.NextDouble();
            return min + (max - min) * number;
        }
    }
}