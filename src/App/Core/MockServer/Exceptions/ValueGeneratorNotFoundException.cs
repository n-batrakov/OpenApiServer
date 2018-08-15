namespace OpenApiServer.Core.MockServer.Exceptions
{
    public class ValueGeneratorNotFoundException : MockServerException
    {
        public ValueGeneratorNotFoundException()
                : base("Unable to find suitable generator.")
        {
        }
    }
}