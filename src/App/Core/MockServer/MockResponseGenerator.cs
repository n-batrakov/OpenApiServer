using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

using JetBrains.Annotations;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    [PublicAPI]
    public class MockResponseGenerator
    {
        private OpenApiDocument Document { get; }

        public MockResponseGenerator(OpenApiDocument document)
        {
            Document = document;
        }

        public MockHttpResponse MockResponse(OpenApiResponse responseSpec, OpenApiMediaType mediaType)
        {
            var hasExample = TryGetExample(mediaType, out var responseBody);
            if (!hasExample)
            {
                responseBody = GenerateExample(mediaType.Schema);
            }

            return new MockHttpResponse(200, responseBody, new Dictionary<string, string>());
        }

        private static bool TryGetExample(OpenApiMediaType mediaType, out string example)
        {
            if (mediaType.Example != null)
            {
                example = SerializeAny(mediaType.Example);
                return true;
            }

            if (mediaType.Examples?.Count > 0)
            {
                example = SerializeAny(mediaType.Examples.First().Value.Value);
                return true;
            }

            if (mediaType.Schema?.Example != null)
            {
                example = SerializeAny(mediaType.Schema.Example);
                return true;
            }
            
            example = null;
            return false;

            string SerializeAny(IOpenApiAny any) => any.ToString();
        }

        private static string GenerateExample(OpenApiSchema schema)
        {
            IEnumerable<IOpenApiExampleProvider> providers = new[] {new PrimitiveExampleProvider(new Random())};

            var textWriter = new StringWriter();
            var jsonWriter = new OpenApiJsonWriter(textWriter);

            providers.WriteValueOrThrow(jsonWriter, schema);
            return textWriter.ToString();
        }
    }

    public static class OpenApiSchemaExtensions
    {
        public static OpenApiSchemaType ConvertTypeToEnum(this OpenApiSchema schema)
        {
            return Enum.Parse<OpenApiSchemaType>(schema.Type, ignoreCase: true);
        }
    }

    public enum OpenApiSchemaType
    {
        Null,
        Boolean,
        Integer,
        Number,
        String,
        Object,
        Array,
    }

    public class OpenApiSchemaTypes
    {
        public static bool IsInteger(string type) => Is(type, "integer");
        public static bool IsNumber(string type) => Is(type, "number");
        public static bool IsNull(string type) => Is(type, "null");
        public static bool IsBoolean(string type) => Is(type, "boolean");
        public static bool IsString(string type) => Is(type, "string");
        public static bool IsObject(string type) => Is(type, "object");
        public static bool IsArray(string type) => Is(type, "array");

        public static bool IsFormattedString(OpenApiSchema schema, string expectedFormat) =>
                IsString(schema.Type) &&
                schema.Format != null &&
                schema.Format.Equals(expectedFormat, StringComparison.OrdinalIgnoreCase);

        private static bool Is(string actual, string expected) =>
                actual.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }


    public static class ExampleProviderCollectionExtensions
    {
        public static void WriteValueOrThrow(this IEnumerable<IOpenApiExampleProvider> providers,
                                             IOpenApiWriter writer,
                                             OpenApiSchema schema)
        {
            var isExampleProvided = providers.Any(x => x.WriteValue(writer, schema));
            if (isExampleProvided)
            {
                return;
            }

            throw new ValueGeneratorNotFoundException();
        }
    }

    public class ValueGeneratorNotFoundException : Exception
    {
        public ValueGeneratorNotFoundException() : base("Unable to find suitable generator.")
        {
            
        }
    }



    public interface IOpenApiExampleProvider
    {
        bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema);
    }


    public class ObjectExampleProvider : IOpenApiExampleProvider
    {
        private IOpenApiExampleProvider[] ExampleProviders { get; }

        private static readonly string[] AdditionalPropertiesExampleNames =
        {
                "DynamicProp1",
                "DynamicProp2",
                "DynamicProp3"
        };

        public ObjectExampleProvider(IEnumerable<IOpenApiExampleProvider> exampleProviders)
        {
            ExampleProviders = exampleProviders.ToArray();
        }

        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!OpenApiSchemaTypes.IsObject(schema.Type))
            {
                return false;
            }

            writer.WriteStartObject();
            {
                WriteStaticProperties(writer, schema);
                WriteAdditionalProperties(writer, schema);
            }
            writer.WriteEndObject();

            return true;
        }

        private void WriteStaticProperties(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.Properties == null)
            {
                return;
            }

            foreach (var property in schema.Properties)
            {
                writer.WritePropertyName(property.Key);
                ExampleProviders.WriteValueOrThrow(writer, property.Value);
            }
        }

        private void WriteAdditionalProperties(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.AdditionalProperties == null)
            {
                return;
            }

            foreach (string property in AdditionalPropertiesExampleNames)
            {
                writer.WritePropertyName(property);
                ExampleProviders.WriteValueOrThrow(writer, schema.AdditionalProperties);
            }
        }
    }

    public class ArrayExampleProvider : IOpenApiExampleProvider
    {
        private IOpenApiExampleProvider[] ExampleProviders { get; }

        public ArrayExampleProvider(IEnumerable<IOpenApiExampleProvider> providers)
        {
            ExampleProviders = providers.ToArray();
        }

        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!OpenApiSchemaTypes.IsArray(schema.Type))
            {
                return false;
            }

            writer.WriteStartArray();
            WriteItems(writer, schema);
            writer.WriteEndArray();

            return true;
        }

        private void WriteItems(IOpenApiWriter writer, OpenApiSchema schema)
        {
            var minItems = schema.MinItems ?? 1;
            for (var i = 0; i < minItems; i++)
            {
                ExampleProviders.WriteValueOrThrow(writer, schema.Items);
            }
        }
    }


    public class PrimitiveExampleProvider : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public PrimitiveExampleProvider(Random random)
        {
            Random = random;
        }

        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            switch (schema.ConvertTypeToEnum())
            {
                case OpenApiSchemaType.Null:
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
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int GetIntValue(OpenApiSchema schema)
        {
            var min = (int)(schema.Minimum ?? 0);
            var max = (int)(schema.Maximum ?? 100);
            return Random.Next(min, max);
        }

        private double GetNumberValue(OpenApiSchema schema)
        {
            var min = (int)(schema.Minimum ?? 0);
            var max = (int)(schema.Maximum ?? 100);
            var number = Random.NextDouble();
            return min + (max - min) * number;
        }
    }

    public class RandomTextExampleProvider : IOpenApiExampleProvider
    {
        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            var isText = OpenApiSchemaTypes.IsString(schema.Type) && schema.Format == null;
            if (!isText)
            {
                return false;
            }

            writer.WriteValue("Lorem Ipsum");

            return true;
        }
    }

    public class GuidExampleProvider : IOpenApiExampleProvider
    {
        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!OpenApiSchemaTypes.IsFormattedString(schema, "guid"))
            {
                return false;
            }

            var guid = Guid.NewGuid().ToString();
            writer.WriteValue(guid);
            return true;
        }
    }

    public class Base64ExampleProvider : IOpenApiExampleProvider
    {
        private const string Base64 = "TW9jayBzZXJ2ZXIgZ2VuZXJhdGVkIGZpbGU=";

        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!OpenApiSchemaTypes.IsFormattedString(schema, "base64"))
            {
                return false;
            }

            writer.WriteValue(Base64);

            return true;
        }
    }

    public class DateTimeExampleProvider : IOpenApiExampleProvider
    {
        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            var isDateTime = OpenApiSchemaTypes.IsFormattedString(schema, "date-time");
            if (!isDateTime)
            {
                return false;
            }

            var value = DateTime.UtcNow.ToString("O");
            writer.WriteValue(value);

            return true;
        }
    }

    public class EnumExampleProvider : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public EnumExampleProvider(Random random)
        {
            Random = random;
        }

        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            var isEnum = OpenApiSchemaTypes.IsString(schema.Type) && schema.Enum != null;
            if (!isEnum)
            {
                return false;
            }

            var valueIndex = Random.Next(0, schema.Enum.Count);
            var value = schema.Enum[valueIndex];
            value.Write(writer);

            return true;
        }
    }

}