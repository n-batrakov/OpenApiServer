using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Exceptions;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Internals
{
    internal static class Extensions
    {
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


        public static void WriteValueOrThrow(this IEnumerable<IMockDataProvider> providers,
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

        public static bool TryWriteValue(this IEnumerable<IMockDataProvider> providers,
                                      IOpenApiWriter writer,
                                      JSchema schema)
        {
            return providers.Any(x => x.TryWriteValue(writer, schema));
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