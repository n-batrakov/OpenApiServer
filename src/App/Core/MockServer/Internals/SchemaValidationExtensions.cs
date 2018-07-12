using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Server.Core.MockServer.Internals
{
    public static class SchemaValidationExtensions
    {
        public static IEnumerable<RequestValidationError> ValidateValue(this OpenApiSchema schema, object value)
        {
            var jsonSchema = schema.ConvertToJSchema();
            var token = JToken.FromObject(value);
            var isValid = token.IsValid(jsonSchema, out IList<string> errors);
            return isValid
                           ? Enumerable.Empty<RequestValidationError>()
                           : errors.Select(ValidationError.SchemaValidationError);
        }

        private static JSchema ConvertToJSchema(this OpenApiSchema schema)
        {
            using (var stringWriter = new StringWriter())
            {
                var jsonWriter = new OpenApiJsonWriter(stringWriter);
                schema.SerializeAsV3WithoutReference(jsonWriter);

                var jsonSchema = stringWriter.ToString();
                return JSchema.Parse(jsonSchema);
            }
        }
    }
}