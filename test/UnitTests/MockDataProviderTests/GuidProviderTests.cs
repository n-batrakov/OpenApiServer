using System;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class GuidProviderTests
    {
        private static GuidProvider Sut => new GuidProvider();

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
        public void IgnoreNonGuidSchema(JSchemaType type, string format = null)
        {
            Sut.EnsureIgnore(new JSchema { Type = type, Format = format });
        }

        [Fact]
        public void CanGenerateGuid()
        {
            var schema = Schema.String("guid");

            var actual = Sut.GetMockData(schema);

            var guid = JToken.Parse(actual).Value<string>();
            Assert.True(Guid.TryParse(guid, out _));
        }
    }
}