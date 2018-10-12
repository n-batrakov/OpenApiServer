using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Internals;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Providers
{
    public class GuidProvider : IMockDataProvider
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