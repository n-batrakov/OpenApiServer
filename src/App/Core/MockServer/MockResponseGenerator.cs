using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration;
using ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators;
using ITExpert.OpenApi.Server.Core.MockServer.Types;
using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockResponseGenerator
    {
        private IOpenApiExampleProvider ExampleProvider { get; }

        public MockResponseGenerator()
        {
            ExampleProvider = new OpenApiExampleProvider();
        }

        public MockHttpResponse MockResponse(OpenApiMediaType mediaType)
        {
            var body = OpenApiSerializer.Serialize(WriteBody);

            return new MockHttpResponse(body);

            void WriteBody(IOpenApiWriter writer)
            {
                var _ = TryWriteExample(writer, mediaType) || ExampleProvider.TryWriteValue(writer, mediaType.Schema);
            }
        }

        private static bool TryWriteExample(IOpenApiWriter writer, OpenApiMediaType mediaType)
        {
            if (mediaType.Example != null)
            {
                mediaType.Example.Write(writer);
                return true;
            }

            if (mediaType.Examples?.Count > 0)
            {
                var example = mediaType.Examples.First().Value.Value;
                example.Write(writer);
                return true;
            }

            return false;
        }
    }
}