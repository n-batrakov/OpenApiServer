using ITExpert.OpenApi.Core.MockServer.Context.Mapping;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Core.MockServer.Generation.Generators
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