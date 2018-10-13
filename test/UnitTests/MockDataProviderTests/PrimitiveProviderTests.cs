using System;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class PrimitiveProviderTests
    {
        private static PrimitiveProvider Sut => new PrimitiveProvider(new Random(42));

        [Theory]
        [InlineData(JSchemaType.Boolean, "true")]
        [InlineData(JSchemaType.Integer, "66")]
        [InlineData(JSchemaType.Null, "null")]
        [InlineData(JSchemaType.Number, "66.81")]
        public void CanGeneratePrimitive(JSchemaType type, string expected)
        {
            var schema = new JSchema {Type = type};

            var actual = Sut.GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }
    }
}