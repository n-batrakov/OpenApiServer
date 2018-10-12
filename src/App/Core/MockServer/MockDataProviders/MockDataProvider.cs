using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Internals;
using OpenApiServer.Core.MockServer.MockDataProviders.Providers;

namespace OpenApiServer.Core.MockServer.MockDataProviders
{
    public class MockDataProvider : IMockDataProvider
    {
        private static readonly IMockDataProvider[] Providers = GetProviders(new Random());

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            return Providers.Any(x => x.TryWriteValue(writer, schema));
        }

        private static IMockDataProvider[] GetProviders(Random rnd)
        {
            // The order between generators may be important.

            var counter = new ObjectDepthCounter(depthThreshold: 5);
            var providers = new List<IMockDataProvider>();

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