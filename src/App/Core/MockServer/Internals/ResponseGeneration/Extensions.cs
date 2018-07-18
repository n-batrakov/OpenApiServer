using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration
{
    internal static class Extensions
    {
        private static readonly Random Random = new Random();

        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, double probability = 0.5) =>
                source.Where(element => Random.NextDouble() <= probability);

        public static void WriteValueOrThrow(this IEnumerable<IOpenApiExampleProvider> providers,
                                             IOpenApiWriter writer,
                                             OpenApiSchema schema)
        {
            var isExampleProvided = providers.Any(x => x.TryWriteValue(writer, schema));
            if (isExampleProvided)
            {
                return;
            }

            throw new ValueGeneratorNotFoundException();
        }

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