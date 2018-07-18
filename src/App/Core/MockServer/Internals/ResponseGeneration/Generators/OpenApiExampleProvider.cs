using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class OpenApiExampleProvider : IOpenApiExampleProvider
    {
        private static readonly IOpenApiExampleProvider[] Providers = GetProviders(new Random());

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            return Providers.Any(x => x.TryWriteValue(writer, schema));
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