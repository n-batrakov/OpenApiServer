namespace OpenApiServer.Core.MockServer.Exceptions
{
    public class HandlerNotFoundException : MockServerConfigurationException
    {
        public HandlerNotFoundException(string handler) : base($"Unable to find handler '{handler}'")
        {
        }
    }
}
