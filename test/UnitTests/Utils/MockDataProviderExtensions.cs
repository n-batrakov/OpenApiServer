using System.IO;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders;

using Xunit.Sdk;

namespace UnitTests.Utils
{
    public static class MockDataProviderExtensions
    {
        public static string GetMockData(this IMockDataProvider sut, JSchema schema)
        {
            if (TryGetMockData(sut, schema, out var data))
            {
                return data;
            }
            throw new XunitException($"{sut.GetType().Name} is expected to write value, but did not.");
        }

        public static void EnsureIgnore(this IMockDataProvider sut, JSchema schema)
        {
            if (TryGetMockData(sut, schema, out _))
            {
                throw new XunitException($"{sut.GetType().Name} is expected to ignore given schema, but did not.");
            }
        }

        public static bool TryGetMockData(this IMockDataProvider sut, JSchema schema, out string mockData)
        {
            var settings = new OpenApiSerializerSettings
                           {
                                   Format = OpenApiFormat.Json,
                                   SpecVersion = OpenApiSpecVersion.OpenApi3_0
                           };
            using (var textWriter = new StringWriter())
            {
                var writer = new OpenApiJsonWriter(textWriter, settings);
                var isWritten = sut.TryWriteValue(writer, schema);
                mockData = isWritten ? textWriter.ToString() : null;

                return isWritten;
            }
        }
    }
}