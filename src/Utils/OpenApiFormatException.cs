using System;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Utils
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