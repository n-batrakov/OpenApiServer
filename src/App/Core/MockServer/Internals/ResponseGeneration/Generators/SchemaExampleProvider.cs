using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    public class SchemaExampleProvider : IOpenApiExampleProvider
    {
        private IOpenApiExampleProvider Provider { get; }

        public SchemaExampleProvider(IOpenApiExampleProvider provider)
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