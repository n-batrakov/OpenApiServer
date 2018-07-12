using System.Collections.Generic;

using JetBrains.Annotations;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    [PublicAPI]
    public class MockHttpResponse
    {
        public int StatusCode { get; }
        public string Body { get; }
        public IDictionary<string, string> Headers { get; }
    }
}