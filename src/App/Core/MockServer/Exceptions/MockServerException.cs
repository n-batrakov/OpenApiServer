using System;

namespace ITExpert.OpenApi.Core.MockServer.Exceptions
{
    public class MockServerException : Exception
    {
        public MockServerException(string message) : base(message)
        {
            
        }
    }
}