using System.Globalization;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

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
            var textWriter = new StringWriter(CultureInfo.InvariantCulture);
            var jsonWriter = new OpenApiJsonWriter(textWriter);

            var _ = TryWriteExample(jsonWriter, mediaType) ||
                    ExampleProvider.TryWriteValue(jsonWriter, mediaType.Schema);

            return new MockHttpResponse(textWriter.ToString());
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