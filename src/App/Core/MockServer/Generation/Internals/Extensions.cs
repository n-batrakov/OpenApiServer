using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Generation.Types;

namespace OpenApiServer.Core.MockServer.Generation.Internals
{
    internal static class Extensions
    {
        private static readonly Random Random = new Random();

        public static IEnumerable<T> TakeRandom<T>(this IList<T> source,
                                                   int minItems = 1,
                                                   int maxItems = -1)
        {
            if (minItems > source.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(minItems), "min items can't be greater than source length.");
            }

            if (source.Count == 0)
            {
                return Enumerable.Empty<T>();
            }

            minItems = minItems < 0 ? 0 : minItems;
            maxItems = maxItems < 0 ? source.Count : maxItems;
            var numberOfItems = Random.Next(minItems, maxItems);
            return Enumerable.Repeat(0, numberOfItems).Select(_ => source[Random.Next(0, source.Count)]);
        }

        public static void AddRange<T>(this IDictionary<string, T> source,
                                       IDictionary<string, T> target,
                                       bool overwriteDuplicateKeys = true)
        {
            foreach (var (key, value) in target)
            {
                if (source.ContainsKey(key))
                {
                    if (overwriteDuplicateKeys)
                    {
                        source[key] = value;
                    }
                    else
                    {
                        throw new ArgumentException($"An element with the key '{key}' already exists in source.");
                    }
                }
                else
                {
                    source.Add(key, value);
                }
            }
        }

        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> value)
        {
            foreach (var element in value)
            {
                source.Add(element);
            }
        }


        public static void WriteValueOrThrow(this IEnumerable<IOpenApiExampleProvider> providers,
                                             IOpenApiWriter writer,
                                             JSchema schema)
        {
            var isExampleProvided = TryWriteValue(providers, writer, schema);
            if (isExampleProvided)
            {
                return;
            }

            throw new ValueGeneratorNotFoundException();
        }

        public static bool TryWriteValue(this IEnumerable<IOpenApiExampleProvider> providers,
                                      IOpenApiWriter writer,
                                      JSchema schema)
        {
            return providers.Any(x => x.TryWriteValue(writer, schema));
        }

        public static OpenApiSchemaType ConvertTypeToEnum(this JSchema schema)
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

        public static void WriteJToken(this IOpenApiWriter writer, JToken token)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (token.Type)
            {
                case JTokenType.String:
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.TimeSpan:
                case JTokenType.Uri:
                    writer.WriteValue(token.ToString());
                    break;
                default:
                    writer.WriteRaw(token.ToString());
                    break;
            }
        }
    }
}