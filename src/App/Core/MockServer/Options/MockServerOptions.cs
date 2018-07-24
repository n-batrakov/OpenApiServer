namespace ITExpert.OpenApi.Server.Core.MockServer.Options
{
    public class MockServerOptions
    {
        public string Host { get; set; }

        public MockServerRouteOptions[] Routes { get; set; }

        public string Route { get; set; }
    }
}