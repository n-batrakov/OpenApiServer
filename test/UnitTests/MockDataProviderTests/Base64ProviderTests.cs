using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class Base64ProviderTests
    {
        [Theory]
        [InlineData(JSchemaType.Array)]
        [InlineData(JSchemaType.Boolean)]
        [InlineData(JSchemaType.Integer)]
        [InlineData(JSchemaType.None)]
        [InlineData(JSchemaType.Null)]
        [InlineData(JSchemaType.Number)]
        [InlineData(JSchemaType.Object)]
        [InlineData(JSchemaType.String)]
        [InlineData(JSchemaType.String, "date-time")]
        public void IgnoreNonBase64Schema(JSchemaType type, string format = null)
        {
            new Base64Provider().EnsureIgnore(new JSchema{Type = type, Format = format});
        }

        [Fact]
        public void CanGenerateBase64()
        {
            var sut = new Base64Provider();
            var expected = "\"TW9jayBzZXJ2ZXIgZ2VuZXJhdGVkIGZpbGU=\"";

            var actual = sut.GetMockData(Schema.Base64());

            JsonAssert.Equal(expected, actual);
        }
    }
}