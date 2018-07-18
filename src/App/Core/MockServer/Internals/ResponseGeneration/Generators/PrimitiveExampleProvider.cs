using System;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    public class PrimitiveExampleProvider : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public PrimitiveExampleProvider(Random random)
        {
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
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

        private int GetIntValue(OpenApiSchema schema)
        {
            //TODO: MultipleOf
            var min = (int)(schema.Minimum ?? 0);
            var max = (int)(schema.Maximum ?? int.MaxValue);
            return Random.Next(min, max);
        }

        private double GetNumberValue(OpenApiSchema schema)
        {
            //TODO: MultipleOf
            var min = (double)(schema.Minimum ?? 0);
            var max = (double)(schema.Maximum ?? decimal.MaxValue);
            var number = Random.NextDouble();
            return min + (max - min) * number;
        }
    }
}