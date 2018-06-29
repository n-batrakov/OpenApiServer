using System;

using ITExpert.OpenApiServer.MockServer.Types;
using ITExpert.OpenApiServer.Util;

using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.Utils
{
    public class HttpRequestValidationContext
    {
        public string Route { get; set; }
        public IHeaderDictionary Headers { get; set; }
        public IQueryCollection Query { get; set; }
        public string Body { get; set; }
        public IFormCollection Form { get; set; }
    }

    public class RequestValidator
    {
        public RequestValidationStatus Validate(HttpRequestValidationContext context,
                                                OpenApiOperation operation,
                                                ReferenceResolver resolver)
        {
            throw new NotImplementedException();
        }
    }
}