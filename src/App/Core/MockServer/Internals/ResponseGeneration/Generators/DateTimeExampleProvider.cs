using System;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals.ResponseGeneration.Generators
{
    public class DateTimeExampleProvider : IOpenApiExampleProvider
    {
        public bool WriteValue(IOpenApiWriter writer, OpenApiSchema schema)
        {
            var isDateTime = schema.IsFormattedString("date-time");
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