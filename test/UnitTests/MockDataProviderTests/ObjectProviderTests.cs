using System.Collections.Generic;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders;
using OpenApiServer.Core.MockServer.MockDataProviders.Internals;
using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.MockDataProviderTests.Fakes;
using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class ObjectProviderTests
    {
        [Theory]
        [InlineData(JSchemaType.None)]
        [InlineData(JSchemaType.Null)]
        [InlineData(JSchemaType.Integer)]
        [InlineData(JSchemaType.Number)]
        [InlineData(JSchemaType.Boolean)]
        [InlineData(JSchemaType.String)]
        [InlineData(JSchemaType.Array)]
        public void IgnoreNonArraySchemas(JSchemaType type)
        {
            Sut().EnsureIgnore(new JSchema { Type = type });
        }

        [Fact]
        public void AnySchemaWithPropertiesIsObject()
        {
            Sut().EnsureWrite(new JSchema {Properties = {["key"] = Schema.Any()}});
        }

        [Fact]
        public void CanGenerateEmptyObject()
        {
            var schema = Schema.Object();
            var expected = "{ }";

            var actual = Sut().GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGenerateObjectWithSingleProperty()
        {
            var schema = Schema.Object(("single", Schema.Any()));
            var elementValue = "42";
            var expected = "{ \"single\": 42 }";

            var actual = Sut(elementValue).GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGenerateNestedObjects()
        {
            var elementValue = "42";
            var schema = Schema.Object(
                    ("first", Schema.Object(
                                ("second", Schema.Object(
                                            ("third", Schema.Any()))))));

            var expected = "{ first: { second: { third: 42 }}}";
            var actual = Sut(elementValue).GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanBreakRecursionCycle()
        {
            var schema = Schema.Object();
            schema.Properties.Add("self", schema);

            var expected = "{self: { self: { self: { self: {}}}}}";
            var actual = Sut().GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }



        private static ObjectProvider Sut(string value = null)
        {
            var staticProvider = new StaticProvider(value, x => x.Type != JSchemaType.Object);
            var providers = new List<IMockDataProvider> { staticProvider };

            var objectProvider = new ObjectProvider(providers, new ObjectDepthCounter(5));
            providers.Add(objectProvider);

            return objectProvider;
        }
    }
}