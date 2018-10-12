using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using OpenApiServer.Core.MockServer.Validation.Types;

using ValidationError = OpenApiServer.Core.MockServer.Validation.Types.ValidationError;

namespace OpenApiServer.Core.MockServer.Validation.Internals
{
    internal static class SchemaValidationExtensions
    {
        public static IEnumerable<HttpValidationError> ValidateValue(this JSchema jsonSchema, object value)
        {
            var token = JToken.FromObject(value);
            return ValidateValue(jsonSchema, token);
        }

        public static IEnumerable<HttpValidationError> ValidateValue(this JSchema jsonSchema, JToken token)
        {
            if (jsonSchema == null)
            {
                return Enumerable.Empty<HttpValidationError>();
            }

            var isValid = token.IsValid(jsonSchema, out IList<string> errors);
            return isValid
                           ? Enumerable.Empty<HttpValidationError>()
                           : errors.Select(ValidationError.SchemaValidationError);
        }
    }
}