using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Core.MockServer.Validation.Types;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using ValidationError = ITExpert.OpenApi.Server.Core.MockServer.Validation.Types.ValidationError;

namespace ITExpert.OpenApi.Server.Core.MockServer.Validation.Internals
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