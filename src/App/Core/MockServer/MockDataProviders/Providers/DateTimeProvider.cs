using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.MockDataProviders.Internals;

namespace OpenApiServer.Core.MockServer.MockDataProviders.Providers
{
    public class DateTimeProvider : IMockDataProvider
    {
        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            if (schema.IsFormattedString("date"))
            {
                var value = DateTime.UtcNow.ToString("yyyy-MM-dd");
                writer.WriteValue(value);
                return true;
            }
            else if (schema.IsFormattedString("date-time"))
            {
                var value = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                writer.WriteValue(value);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}