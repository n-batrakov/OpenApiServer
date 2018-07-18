using System;
using System.Collections.Generic;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    public class SomeOfExampleProivder : IOpenApiExampleProvider
    {
        private Random Random { get; }
        private IReadOnlyCollection<IOpenApiExampleProvider> Providers { get; }

        public SomeOfExampleProivder(IReadOnlyCollection<IOpenApiExampleProvider> providers, Random random)
        {
            Providers = providers;
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.AnyOf != null && schema.AnyOf.Count > 0)
            {
                foreach (var anyOfSchema in schema.AnyOf.TakeRandom())
                {
                    Providers.WriteValueOrThrow(writer, anyOfSchema);
                }

                return true;
            }

            if (schema.OneOf != null && schema.OneOf.Count > 0)
            {
                var index = Random.Next(0, schema.OneOf.Count);
                var oneOfSchema = schema.OneOf[index];
                Providers.WriteValueOrThrow(writer, oneOfSchema);

                return true;
            }

            return false;
        }
    }
}