using System.Linq;

using Microsoft.OpenApi.Writers;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Generation.Types;
using OpenApiServer.Utils;

namespace OpenApiServer.Core.MockServer.Generation
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