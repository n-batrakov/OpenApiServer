using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Generation.Generators
{
    public class SchemaExampleGenerator : IOpenApiExampleProvider
    {
        private IOpenApiExampleProvider Provider { get; }

        public SchemaExampleGenerator(IOpenApiExampleProvider provider)
        {
            Provider = provider;
        }

        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (schema.Example != null)
            {
                schema.Example.Write(writer);
                return true;
            }

            if (schema.Default != null)
            {
                schema.Default.Write(writer);
                return true;
            }

            return Provider.TryWriteValue(writer, schema);
        }
    }
}