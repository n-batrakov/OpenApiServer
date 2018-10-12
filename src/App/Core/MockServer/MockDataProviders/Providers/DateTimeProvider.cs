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
            var isDateTime = schema.IsFormattedString("date-time") || schema.IsFormattedString("date");
            if (!isDateTime)
            {
                return false;
            }

            var value = DateTime.UtcNow.ToString("O");
            writer.WriteValue(value);

            return true;
        }
    }
}