using System;
using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders;

namespace UnitTests.MockDataProviderTests.Fakes
{
    public class StaticProvider : IMockDataProvider
    {
        private string Value { get; }
        private Func<JSchema, bool> ShouldWrite { get; }

        public StaticProvider(string value)
        {
            Value = value;
        }

        public StaticProvider(string value, Func<JSchema, bool> shouldWrite)
        {
            Value = value;
            ShouldWrite = shouldWrite;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            if (ShouldWrite == null || !ShouldWrite(schema))
            {
                return false;
            }

            writer.WriteRaw(Value);
            return true;
        }
    }
}