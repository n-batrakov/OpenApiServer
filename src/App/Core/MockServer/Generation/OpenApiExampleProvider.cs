using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Generation.Generators;
using OpenApiServer.Core.MockServer.Generation.Internals;

namespace OpenApiServer.Core.MockServer.Generation
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
            // The order between generators may be important.

            var counter = new ObjectDepthCounter(depthThreshold: 5);
            var providers = new List<IOpenApiExampleProvider>();

            providers.Add(new SchemaExampleGenerator());

            
            providers.Add(new EnumGenerator(rnd));

            providers.Add(new PrimitiveGenerator(rnd));
            providers.Add(new AnyGenerator());

            providers.Add(new GuidGenerator());
            providers.Add(new Base64Generator());
            providers.Add(new DateTimeGenerator());
            providers.Add(new TextGenerator(rnd));

            providers.Add(new ArrayGenerator(providers));

            providers.Add(new CombinedGenerator(providers));
            providers.Add(new ObjectGenerator(providers, counter));

            return providers.ToArray();
        }
    }
}