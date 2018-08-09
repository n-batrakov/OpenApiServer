using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Mapping;
using OpenApiServer.Core.MockServer.Generation.Internals;

namespace OpenApiServer.Core.MockServer.Generation.Generators
{
    public class SchemaExampleGenerator : IOpenApiExampleProvider
    {
        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            var example = schema.GetExample();
            if (example == null)
            {
                return false;
            }

            writer.WriteJToken(example);
            return true;
        }
    }
}