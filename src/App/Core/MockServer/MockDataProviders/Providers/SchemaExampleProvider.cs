using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Context.Mapping;
using OpenApiServer.Core.MockServer.MockDataProviders.Internals;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Providers
{
    public class SchemaExampleProvider : IMockDataProvider
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