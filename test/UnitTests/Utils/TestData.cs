using System.IO;

using ITExpert.OpenApi.Core.MockServer.Validation;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

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

        public static RequestValidationError GetInvalidParameterTypeSchemaError(
                string expectedType,
                string actualType,
                string path = "")
        {
            var msg = $"Invalid type. Expected {expectedType} but got {actualType}. Path '{path}'.";
            return ValidationError.SchemaValidationError(msg);
        }

        public static RequestValidationError GetMissingParameterSchemaError(string parameterName, string path = "")
        {
            var msg = $"Required properties are missing from object: {parameterName}. Path '{path}'.";
            return ValidationError.SchemaValidationError(msg);
        }
    }
}