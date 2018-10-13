using System;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class EnumProviderTests
    {
        private static EnumProvider Sut => new EnumProvider(new Random(42));

        [Theory]
        [InlineData(JSchemaType.Array)]
        [InlineData(JSchemaType.Boolean)]
        [InlineData(JSchemaType.Integer)]
        [InlineData(JSchemaType.None)]
        [InlineData(JSchemaType.Null)]
        [InlineData(JSchemaType.Number)]
        [InlineData(JSchemaType.Object)]
        [InlineData(JSchemaType.String)]
        public void IgnoreNonEnumSchema(JSchemaType type)
        {
            Sut.EnsureIgnore(new JSchema {Type = type});
        }

        [Fact]
        public void CanGenerateEnumWithSingleValue()
        {
            var schema = Schema.Enum("single");
            var expected = "\"single\"";

            var actual = Sut.GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGenerateEnumWithMultipleValues()
        {
            var schema = Schema.Enum("one", "two", "three");
            var expected = "\"three\"";

            var actual = Sut.GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }
    }
}