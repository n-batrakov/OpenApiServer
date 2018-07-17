using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration;
using ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators;
using ITExpert.OpenApi.Server.Core.MockServer.Types;

using JetBrains.Annotations;

using Microsoft.OpenApi.Any;
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
            var hasExample = TryGetExample(mediaType, out var responseBody);
            if (!hasExample)
            {
                responseBody = GenerateExample(mediaType.Schema);
            }

            return new MockHttpResponse(200, responseBody, new Dictionary<string, string>());
        }

        private static bool TryGetExample(OpenApiMediaType mediaType, out string example)
        {
            if (mediaType.Example != null)
            {
                example = SerializeAny(mediaType.Example);
                return true;
            }

            if (mediaType.Examples?.Count > 0)
            {
                example = SerializeAny(mediaType.Examples.First().Value.Value);
                return true;
            }

            example = null;
            return false;

            string SerializeAny(IOpenApiAny any) => any.ToString();
        }

        private static string GenerateExample(OpenApiSchema schema)
        {
            var textWriter = new StringWriter();
            var jsonWriter = new OpenApiJsonWriter(textWriter);

            ExampleProviders.WriteValueOrThrow(jsonWriter, schema);
            return textWriter.ToString();
        }

        private static IOpenApiExampleProvider[] GetProviders(Random rnd)
        {
            var providers = new List<IOpenApiExampleProvider>(GetSimpleProviders());
            providers.Add(new ObjectExampleProvider(providers));
            providers.Add(new SomeOfExampleProivder(providers, rnd));
            providers.Add(new ArrayExampleProvider(providers));

            return providers.Select(Wrap).Reverse().ToArray();

            IOpenApiExampleProvider Wrap(IOpenApiExampleProvider x) =>
                    new SchemaExampleProvider(x);

            IEnumerable<IOpenApiExampleProvider> GetSimpleProviders()
            {
                yield return new PrimitiveExampleProvider(rnd);
                yield return new GuidExampleProvider();
                yield return new Base64ExampleProvider();
                yield return new DateTimeExampleProvider();
                yield return new TextExampleProvider(rnd);
                yield return new EnumExampleProvider(rnd);
            }
        }
    }
}