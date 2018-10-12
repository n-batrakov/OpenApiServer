namespace OpenApiServer.Core.MockServer.Validation.Types
{
    public static class ValidationError
    {
        public static HttpValidationError ParameterRequired(string parameter)
        { 
            var code = "ParameterRequired";
            var description = "Parameter is required but was not found in the request.";
            return new HttpValidationError(code, description, parameter);
        }

        public static HttpValidationError ParameterMustHaveValue(string parameter)
        {
            var code = "ParameterMustHaveValue";
            var description = "Parameter must have value but none was found.";
            return new HttpValidationError(code, description, parameter);
        }

        public static HttpValidationError InvalidParameter(string parameter, params HttpValidationError[] errors)
        {
            var code = "InvalidParameter";
            var description = "Parameter does not match the schema.";
            return new HttpValidationError(code, description, parameter, errors);
        }

        public static HttpValidationError SchemaValidationError(string message)
        {
            var code = "SchemaValidationError";
            return new HttpValidationError(code, message);
        }

        public static HttpValidationError BodyRequired()
        {
            var code = "BodyRequired";
            var description = "Body is required but was not found in the request";
            return new HttpValidationError(code, description);
        }

        public static HttpValidationError InvalidBody(params HttpValidationError[] errors)
        {
            var code = "InvalidRequestBody";
            var description = "Request body does not match the schema.";
            return new HttpValidationError(code, description, errors);
        }

        public static HttpValidationError UnexpectedContentType(string contentType)
        {
            var code = "UnexpectedContentType";
            var description = $"Content-Type '{contentType}' is not expected. See supported content types.";
            return new HttpValidationError(code, description);
        }
    }
}