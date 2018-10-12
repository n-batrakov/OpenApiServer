using System.IO;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

using OpenApiServer.Core.MockServer.Validation.Types;

namespace UnitTests.Utils
{
    public static class TestData
    {
        public static readonly OpenApiDocument Petstore = GetDocument("data/petstore.yml");

        private static OpenApiDocument GetDocument(string path)
        {
            var reader = new OpenApiStreamReader();
            using (var streamReader = new StreamReader(path))
            {
                return reader.Read(streamReader.BaseStream, out _);
            }
        }

        public static HttpValidationError GetInvalidParameterTypeSchemaError(
                string expectedType,
                string actualType,
                string path = "")
        {
            var msg = $"Invalid type. Expected {expectedType} but got {actualType}. Path '{path}'.";
            return ValidationError.SchemaValidationError(msg);
        }

        public static HttpValidationError GetMissingParameterSchemaError(string parameterName, string path = "")
        {
            var msg = $"Required properties are missing from object: {parameterName}. Path {path}.";
            return ValidationError.SchemaValidationError(msg);
        }
    }
}