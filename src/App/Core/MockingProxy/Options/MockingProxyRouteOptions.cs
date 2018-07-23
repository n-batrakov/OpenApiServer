namespace ITExpert.OpenApi.Server.Core.MockingProxy.Options
{
    public class MockingProxyRouteOptions
    {
        public string Path { get; set; }
        public MockingProxyHttpMethod Method { get; set; }
        public bool Mock { get; set; }
        public MockingProxyValidationMode Validate { get; set; }
        public ushort Latency { get; set; }
    }
}