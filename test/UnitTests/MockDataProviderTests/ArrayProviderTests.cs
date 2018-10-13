using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.MockDataProviderTests.Fakes;
using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class ArrayProviderTests
    {
        private static ArrayProvider Sut(string value) => 
                new ArrayProvider(new[] {new StaticProvider(value)});

        [Theory]
        [InlineData(JSchemaType.None)]
        [InlineData(JSchemaType.Null)]
        [InlineData(JSchemaType.Integer)]
        [InlineData(JSchemaType.Number)]
        [InlineData(JSchemaType.Boolean)]
        [InlineData(JSchemaType.String)]
        [InlineData(JSchemaType.Object)]
        public void IgnoreNonArraySchemas(JSchemaType type)
        {
            Sut(null).EnsureIgnore(new JSchema {Type = type});
        }

        [Fact]
        public void CanWriteArray()
        {
            var schema = Schema.Array(Schema.Any());
            var elementValue = "42";
            var expected = "[42]";

            var actual = Sut(elementValue).GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanConsiderMinItems()
        {
            var schema = Schema.Array(Schema.Any(), minItems: 3);
            var elementValue = "42";
            var expected = "[42, 42, 42]";

            var actual = Sut(elementValue).GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }
    }
}