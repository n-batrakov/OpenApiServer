using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation
{
    internal static class SchemaValidationExtensions
    {
        public static IEnumerable<RequestValidationError> ValidateValue(this OpenApiSchema schema, object value)
        {
            var jsonSchema = schema.ConvertToJSchema();
            if (jsonSchema == null)
            {
                return Enumerable.Empty<RequestValidationError>();
            }

            var token = JToken.FromObject(value);
            var isValid = token.IsValid(jsonSchema, out IList<string> errors);
            return isValid
                           ? Enumerable.Empty<RequestValidationError>()
                           : errors.Select(ValidationError.SchemaValidationError);
        }

        private static JSchema ConvertToJSchema(this OpenApiSchema schema)
        {
            var converter = new OpenApiSchemaConverter();
            return converter.Convert(schema);
        }
    }
}