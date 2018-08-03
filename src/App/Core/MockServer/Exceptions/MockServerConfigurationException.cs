namespace ITExpert.OpenApi.Core.MockServer.Exceptions
{
    public class MockServerConfigurationException : MockServerException
    {
        public MockServerConfigurationException(string message)
                : base($"Configuration is invalid. {message}")
        {
        }
    }
}