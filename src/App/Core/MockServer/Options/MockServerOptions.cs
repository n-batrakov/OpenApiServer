namespace OpenApiServer.Core.MockServer.Options
{
    public class MockServerOptions
    {
        public MockServerRouteOptions[] Routes { get; set; }

        public string MockServerHost { get; set; }
    }
}