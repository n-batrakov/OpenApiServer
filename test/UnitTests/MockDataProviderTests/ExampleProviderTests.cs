using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class ExampleProviderTests
    {
        private static SchemaExampleProvider Sut => new SchemaExampleProvider();

        [Fact]
        public void IgnoreSchemaWithoutExample()
        {
            Sut.EnsureIgnore(new JSchema());
        }

        [Fact]
        public void CanReturnSchemaExample()
        {
            var schema = new JSchema {ExtensionData = {{"x-example", 42}}};
            var expected = "42";

            var actual = Sut.GetMockData(schema);

            JsonAssert.Equal(expected, actual);
        }
    }
}