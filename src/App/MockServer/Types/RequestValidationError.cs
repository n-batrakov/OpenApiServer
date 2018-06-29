using System.Collections.Generic;

namespace ITExpert.OpenApiServer.MockServer
{
    public class RequestValidationError
    {
        public string Code { get; }
        public string Description { get; }
        public string Parameter { get; }
        public IEnumerable<RequestValidationError> Inner { get; }

        public RequestValidationError(string code,
                                      string description,
                                      IEnumerable<RequestValidationError> inner)
                : this(code, description, null, inner)
        {
        }

        public RequestValidationError(string code,
                                      string description,
                                      string parameter,
                                      IEnumerable<RequestValidationError> inner)
        {
            Code = code;
            Description = description;
            Parameter = parameter;
            Inner = inner;
        }
    }
}