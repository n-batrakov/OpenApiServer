namespace ITExpert.OpenApi.Core.MockServer.Validation.Types
{
    public static class ValidationError
    {
        public static RequestValidationError ParameterRequired(string parameter)
        { 
            var code = "ParameterRequired";
            var description = "Parameter is required but was not found in the request.";
            return new RequestValidationError(code, description, parameter);
        }

        public static RequestValidationError ParameterMustHaveValue(string parameter)
        {
            var code = "ParameterMustHaveValue";
            var description = "Parameter must have value but none was found.";
            return new RequestValidationError(code, description, parameter);
        }

        public static RequestValidationError InvalidParameter(string parameter, params RequestValidationError[] errors)
        {
            var code = "InvalidParameter";
            var description = "Parameter does not match the schema.";
            return new RequestValidationError(code, description, parameter, errors);
        }

        public static RequestValidationError SchemaValidationError(string message)
        {
            var code = "SchemaValidationError";
            return new RequestValidationError(code, message);
        }

        public static RequestValidationError BodyRequired()
        {
            var code = "BodyRequired";
            var description = "Body is required but was not found in the request";
            return new RequestValidationError(code, description);
        }

        public static RequestValidationError InvalidBody(params RequestValidationError[] errors)
        {
            var code = "InvalidRequestBody";
            var description = "Request body does not match the schema.";
            return new RequestValidationError(code, description, errors);
        }

        public static RequestValidationError UnexpectedContentType(string contentType)
        {
            var code = "UnexpectedContentType";
            var description = $"Content-Type '{contentType}' is not expected. See supported content types.";
            return new RequestValidationError(code, description);
        }
    }
}