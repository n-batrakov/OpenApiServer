using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.ExampleProviders.Internals;

namespace OpenApiServer.Core.MockServer.ExampleProviders.Providers
{
    public class GuidProvider : IOpenApiExampleProvider
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