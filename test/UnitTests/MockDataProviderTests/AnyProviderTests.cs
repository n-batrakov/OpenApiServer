using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.MockDataProviderTests
{
    public class AnyProviderTests
    {
        [Fact]
        public void CanWriteAny()
        {
            var expected = "{}";

            var actual = new AnyProvider().GetMockData(new JSchema());

            JsonAssert.Equal(expected, actual);
        }
    }
}
