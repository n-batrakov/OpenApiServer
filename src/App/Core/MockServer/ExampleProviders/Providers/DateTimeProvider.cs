using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.ExampleProviders.Internals;

namespace OpenApiServer.Core.MockServer.ExampleProviders.Providers
{
    public class DateTimeProvider : IOpenApiExampleProvider
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