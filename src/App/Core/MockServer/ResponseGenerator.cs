using System;

using JetBrains.Annotations;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
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