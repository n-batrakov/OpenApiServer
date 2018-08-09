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
        public static IEnumerable<RequestValidationError> ValidateValue(this JSchema jsonSchema, object value)
        {
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
    }
}