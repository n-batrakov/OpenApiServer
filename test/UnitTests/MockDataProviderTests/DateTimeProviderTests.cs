using System;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class DateTimeProviderTests
    {
        private static DateTimeProvider Sut => new DateTimeProvider();

        [Theory]
        [InlineData(JSchemaType.Array)]
        [InlineData(JSchemaType.Boolean)]
        [InlineData(JSchemaType.Integer)]
        [InlineData(JSchemaType.None)]
        [InlineData(JSchemaType.Null)]
        [InlineData(JSchemaType.Number)]
        [InlineData(JSchemaType.Object)]
        [InlineData(JSchemaType.String)]
        [InlineData(JSchemaType.String, "base64")]
        public void IgnoreNonDateTimeSchema(JSchemaType type, string format = null)
        {
            Sut.EnsureIgnore(new JSchema {Type = type, Format = format});
        }

        [Fact]
        public void CanGenerateDateTime()
        {
            var expected = $"\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\"";

            var actual = Sut.GetMockData(Schema.DateTime());

            JsonAssert.Equal(expected, actual);
        }

        [Fact]
        public void CanGenerateDateOnly()
        {
            var expected = $"\"{DateTime.UtcNow:yyyy-MM-dd}\"";

            var actual = Sut.GetMockData(Schema.String("date"));

            JsonAssert.Equal(expected, actual);
        }
    }
}