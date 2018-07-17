using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration
{
    public static class ExampleProviderCollectionExtensions
    {
        public static void WriteValueOrThrow(this IEnumerable<IOpenApiExampleProvider> providers,
                                             IOpenApiWriter writer,
                                             OpenApiSchema schema)
        {
            var isExampleProvided = providers.Any(x => x.WriteValue(writer, schema));
            if (isExampleProvided)
            {
                return;
            }

            throw new ValueGeneratorNotFoundException();
        }
    }
}