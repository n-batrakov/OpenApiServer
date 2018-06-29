using System;
using System.Collections.Generic;

using ITExpert.OpenApiServer.Util;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApiServer.Utils
{
    public class MockHttpResponse
    {
        public int StatusCode { get; }
        public string Body { get; }
        public IDictionary<string, string> Headers { get; }
    }

    public class ResponseGenerator
    {
        public MockHttpResponse MockResponse(OpenApiResponse spec,
                                             string mediaType,
                                             ReferenceResolver resolver)
        {
            throw new NotImplementedException();
        }
    }
}