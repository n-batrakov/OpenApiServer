using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders;

namespace UnitTests.MockDataProviderTests.Fakes
{
    public class StaticProvider : IMockDataProvider
    {
        private string Value { get; }

        public StaticProvider(string value)
        {
            Value = value;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            writer.WriteRaw(Value);
            return true;
        }
    }
}