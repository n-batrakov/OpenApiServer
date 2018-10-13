using System;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class TextProviderTests
    {
        private static TextProvider Sut => new TextProvider(new Random(42));

        [Theory]
        [InlineData(JSchemaType.Array)]
        [InlineData(JSchemaType.Boolean)]
        [InlineData(JSchemaType.Integer)]
        [InlineData(JSchemaType.None)]
        [InlineData(JSchemaType.Null)]
        [InlineData(JSchemaType.Number)]
        [InlineData(JSchemaType.Object)]
        public void IgnoreNonTextSchema(JSchemaType type, string format = null)
        {
            Sut.EnsureIgnore(new JSchema { Type = type, Format = format });
        }

        [Fact]
        public void CanWriteText()
        {
            var schema = Schema.String();
            var expected = "\"M\"";

            var actual = Sut.GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanConsiderMinLength()
        {
            var schema = Schema.String(minLength: 10);
            var expected = "\"Melius ali\"";

            var actual = Sut.GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanConsiderMaxLength()
        {
            var schema = Schema.String(maxLength: 3);
            var expected = "\"\"";

            var actual = Sut.GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }
    }
}