using System.Linq;

using ITExpert.OpenApi.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Core.MockServer.Generation
{
    public class MockResponseGenerator
    {
        private IOpenApiExampleProvider ExampleProvider { get; }

        public MockResponseGenerator()
        {
            ExampleProvider = new OpenApiExampleProvider();
        }

        public MockHttpResponse MockResponse(RequestContextResponse mediaType)
        {
            var body = OpenApiSerializer.Serialize(WriteBody);

            return new MockHttpResponse(body);

            void WriteBody(IOpenApiWriter writer)
            {
                var _ = TryWriteExample(writer, mediaType) || ExampleProvider.TryWriteValue(writer, mediaType.Schema);
            }
        }

        private static bool TryWriteExample(IOpenApiWriter writer, RequestContextResponse mediaType)
        {
            if (mediaType.Examples?.Count > 0)
            {
                var example = mediaType.Examples.First();
                writer.WriteRaw(example);
                return true;
            }

            return false;
        }
    }
}