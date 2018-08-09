using System;

namespace OpenApiServer.Core.MockServer.Exceptions
{
    public class MockServerException : Exception
    {
        public MockServerException(string message) : base(message)
        {
            
        }
    }
}