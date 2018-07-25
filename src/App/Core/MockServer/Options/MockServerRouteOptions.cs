namespace ITExpert.OpenApi.Server.Core.MockServer.Options
{
    public class MockServerRouteOptions
    {
        public string Path { get; set; }
        public MockServerOptionsHttpMethod Method { get; set; }
        public bool Mock { get; set; }
        public MockServerOptionsValidationMode Validate { get; set; }
        public ushort Latency { get; set; }

        public bool ShouldValidateRequest => Validate == MockServerOptionsValidationMode.All ||
                                               Validate == MockServerOptionsValidationMode.Request;

        public bool ShouldValidateResponse => Validate == MockServerOptionsValidationMode.All ||
                                                Validate == MockServerOptionsValidationMode.Response;

        public static MockServerRouteOptions Default =>
                new MockServerRouteOptions
                {
                        Path = ".*",
                        Method = MockServerOptionsHttpMethod.Any,
                        Latency = 0,
                        Mock = false,
                        Validate = MockServerOptionsValidationMode.None
                };
    }
}