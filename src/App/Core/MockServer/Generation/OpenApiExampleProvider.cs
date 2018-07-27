using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Generation.Generators;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Server.Core.MockServer.Generation
{
    public class OpenApiExampleProvider : IOpenApiExampleProvider
    {
        private static readonly IOpenApiExampleProvider[] Providers = GetProviders(new Random());

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            return Providers.Any(x => x.TryWriteValue(writer, schema));
        }

        private static IOpenApiExampleProvider[] GetProviders(Random rnd)
        {
            var counter = new ObjectDepthCounter(depthThreshold: 5);
            var providers = new List<IOpenApiExampleProvider>();

            // The order between this and others is important.
            providers.Add(new EnumGenerator(rnd));

            providers.Add(new PrimitiveGenerator(rnd));
            providers.Add(new AnyGenerator());

            providers.Add(new TextGenerator(rnd));
            providers.Add(new GuidGenerator());
            providers.Add(new Base64Generator());
            providers.Add(new DateTimeGenerator());

            providers.Add(new ArrayGenerator(providers));

            providers.Add(new CombinedGenerator(providers));
            providers.Add(new ObjectGenerator(providers, counter));

            return providers.ToArray();
        }
    }
}