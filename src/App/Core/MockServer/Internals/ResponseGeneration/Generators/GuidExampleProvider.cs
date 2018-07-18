using System;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    public class GuidExampleProvider : IOpenApiExampleProvider
    {
        public bool TryWriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            if (!schema.IsFormattedString("guid"))
            {
                return false;
            }

            var guid = Guid.NewGuid().ToString();
            writer.WriteValue(guid);
            return true;
        }
    }
}