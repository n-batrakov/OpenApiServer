using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration;
using ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators;
using ITExpert.OpenApi.Server.Core.MockServer.Types;

using JetBrains.Annotations;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    [PublicAPI]
    public class MockResponseGenerator
    {
        private OpenApiDocument Document { get; }
        private static readonly IOpenApiExampleProvider[] ExampleProviders = GetProviders(new Random());

        public MockResponseGenerator(OpenApiDocument document)
        {
            Document = document;
        }

        public MockHttpResponse MockResponse(OpenApiResponse responseSpec, OpenApiMediaType mediaType)
        {
            var textWriter = new StringWriter(CultureInfo.InvariantCulture);
            var jsonWriter = new OpenApiJsonWriter(textWriter);

            var hasExample = TryGetExample(jsonWriter, mediaType);
            if (!hasExample)
            {
                ExampleProviders.WriteValueOrThrow(jsonWriter, mediaType.Schema);
            }

            return new MockHttpResponse(200, textWriter.ToString(), new Dictionary<string, string>());
        }

        private static bool TryGetExample(IOpenApiWriter writer, OpenApiMediaType mediaType)
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

        private static IOpenApiExampleProvider[] GetProviders(Random rnd)
        {
            var providers = new List<IOpenApiExampleProvider>();

            // The order between this and others is important.
            providers.Add(new EnumExampleProvider(rnd));

            providers.Add(new PrimitiveExampleProvider(rnd));
            providers.Add(new AnyExampleProvider());

            providers.Add(new TextExampleProvider(rnd));
            providers.Add(new GuidExampleProvider());
            providers.Add(new Base64ExampleProvider());
            providers.Add(new DateTimeExampleProvider());

            providers.Add(new ArrayExampleProvider(providers));

            // The order between these two is important.
            providers.Add(new ObjectExampleProvider(providers, rnd));
            providers.Add(new SomeOfExampleProivder(providers, rnd));


            return providers.Select(Wrap).ToArray();

            IOpenApiExampleProvider Wrap(IOpenApiExampleProvider x) =>
                    new SchemaExampleProvider(x);
        }
    }
}