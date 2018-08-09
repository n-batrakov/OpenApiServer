using System.Collections.Generic;

namespace OpenApiServer.Core.MockServer.Generation.Types
{
    public class MockHttpResponse
    {
        public string Body { get; }
        public IDictionary<string, string> Headers { get; }

        public MockHttpResponse(string body)
        {
            Body = body;
            Headers = new Dictionary<string, string>();
        }

        public MockHttpResponse(string body, IDictionary<string, string> headers)
        {
            Body = body;
            Headers = headers;
        }
    }
}