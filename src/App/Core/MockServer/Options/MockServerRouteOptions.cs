namespace ITExpert.OpenApi.Server.Core.MockServer.Options
{
    public class MockServerRouteOptions
    {
        public string Path { get; set; }
        public MockServerOptionsHttpMethod Method { get; set; }
        public bool Mock { get; set; }
        public MockServerOptionsValidationMode Validate { get; set; }
        public ushort Latency { get; set; }
    }
}