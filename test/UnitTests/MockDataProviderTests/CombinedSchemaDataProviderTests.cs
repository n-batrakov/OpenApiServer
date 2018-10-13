using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders;
using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class CombinedSchemaDataProviderTests
    {
        private static CombinedSchemaDataProvider Sut() =>
                new CombinedSchemaDataProvider(new[] {new Provider()}, new Random(42));

        [Fact]
        public void IgnoreNonCombinedSchema()
        {
            Sut().EnsureIgnore(new JSchema());
        }

        [Fact]
        public void CanGenerateAnyOf()
        {
            var schema = new JSchema
                         {
                                 AnyOf = {Schema.Int(), Schema.String(), Schema.Boolean()}
                         };
            var expected = "true";

            var actual = Sut().GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGenerateOneOf()
        {
            var schema = new JSchema
                         {
                                 OneOf = { Schema.Int(), Schema.String(), Schema.Boolean() }
                         };
            var expected = "true";

            var actual = Sut().GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGenerateAllOf()
        {
            var one = Schema.Object(("one", Schema.Int()));
            var two = Schema.Object(("two", Schema.String()));
            var three = Schema.Object(("three", Schema.Boolean()));
            var schema = new JSchema {AllOf = {one, two, three}};
            var expected = "{one: 42, two: 'text', three: true}";

            var actual = Sut().GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        private class Provider : IMockDataProvider
        {
            public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
            {
                switch (schema.Type)
                {
                    case JSchemaType.String:
                        writer.WriteValue("text");
                        return true;
                    case JSchemaType.Number:
                    case JSchemaType.Integer:
                        writer.WriteValue(42);
                        return true;
                    case JSchemaType.Boolean:
                        writer.WriteValue(true);
                        return true;
                    case JSchemaType.None:
                        return true;
                    case JSchemaType.Null:
                        writer.WriteNull();
                        return true;
                    case null:
                        return true;
                    case JSchemaType.Object:
                        writer.WriteStartObject();
                        foreach (var (propName, propSchema) in schema.Properties)
                        {
                            writer.WritePropertyName(propName);
                            TryWriteValue(writer, propSchema);
                        }
                        writer.WriteEndObject();
                        return true;
                    case JSchemaType.Array:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}