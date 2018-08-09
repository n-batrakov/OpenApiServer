using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Generation.Internals;

namespace OpenApiServer.Core.MockServer.Generation.Generators
{
    public class GuidGenerator : IOpenApiExampleProvider
    {
        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
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