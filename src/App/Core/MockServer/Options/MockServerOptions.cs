namespace ITExpert.OpenApi.Server.Core.MockServer.Options
{
    public class MockServerOptions
    {
        public MockServerRouteOptions[] Routes { get; set; }

        public string PathPattern { get; set; }
    }
}