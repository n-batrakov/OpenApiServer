using System;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.Exceptions
{
    public class OpenApiFormatException : FormatException
    {
        public OpenApiFormatException(string message)
                : base($"Invalid OpenAPI format: {message}")
        {
        }

        public OpenApiFormatException(OpenApiError error)
                : base($"Invalid OpenAPI format at [{error.Pointer}]: {error.Message}")
        {
        }
    }
}