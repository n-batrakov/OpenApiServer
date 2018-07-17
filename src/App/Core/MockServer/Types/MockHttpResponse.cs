using System.Collections.Generic;

using JetBrains.Annotations;

namespace ITExpert.OpenApi.Server.Core.MockServer.Types
{
    [PublicAPI]
    public class MockHttpResponse
    {
        public int StatusCode { get; }
        public string Body { get; }
        public IDictionary<string, string> Headers { get; }

        public MockHttpResponse(int statusCode, string body, IDictionary<string, string> headers)
        {
            StatusCode = statusCode;
            Body = body;
            Headers = headers;
        }
    }
}