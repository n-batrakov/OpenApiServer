using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Internals
{
    [PublicAPI]
    public class MockHttpResponse
    {
        public int StatusCode { get; }
        public string Body { get; }
        public IDictionary<string, string> Headers { get; }
    }

    [PublicAPI]
    public class ResponseGenerator
    {
        private OpenApiDocument Document { get; }

        public ResponseGenerator(OpenApiDocument document)
        {
            Document = document;
        }

        public MockHttpResponse MockResponse(OpenApiResponse spec,
                                             string mediaType)
        {
            throw new NotImplementedException();
        }
    }
}