using System;

namespace ITExpert.OpenApi.Server.Core.MockServer.Exceptions
{
    public class MockServerException : Exception
    {
        public MockServerException(string message) : base(message)
        {
            
        }
    }
}