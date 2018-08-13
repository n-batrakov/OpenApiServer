using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.ExampleProviders.Internals;
using OpenApiServer.Core.MockServer.ExampleProviders.Providers;

namespace OpenApiServer.Core.MockServer.ExampleProviders
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

            providers.Add(new SchemaExampleProvider());

            
            providers.Add(new EnumProvider(rnd));

            providers.Add(new PrimitiveProvider(rnd));
            providers.Add(new AnyProvider());

            providers.Add(new GuidProvider());
            providers.Add(new Base64Provider());
            providers.Add(new DateTimeProvider());
            providers.Add(new TextProvider(rnd));

            providers.Add(new ArrayProvider(providers));

            providers.Add(new CombinedGenerator(providers));
            providers.Add(new ObjectProvider(providers, counter));

            return providers.ToArray();
        }
    }
}