namespace ITExpert.OpenApi.Server.Core.MockServer.Options
{
    public class MockServerRouteOptions
    {
        public string Path { get; set; }
        public MockServerOptionsHttpMethod Method { get; set; }
        public bool? Mock { get; set; }
        public MockServerOptionsValidationMode Validate { get; set; }
        public ushort Delay { get; set; }
        public string Host { get; set; }

        public bool ShouldValidateRequest => Validate == MockServerOptionsValidationMode.All ||
                                               Validate == MockServerOptionsValidationMode.Request;

        public bool ShouldValidateResponse => Validate == MockServerOptionsValidationMode.All ||
                                                Validate == MockServerOptionsValidationMode.Response;

        public static MockServerRouteOptions Default =>
                new MockServerRouteOptions
                {
                        Path = ".*",
                        Method = MockServerOptionsHttpMethod.Any,
                        Delay = 0,
                        Mock = false,
                        Validate = MockServerOptionsValidationMode.Undefined
                };

        public MockServerRouteOptions Merge(MockServerRouteOptions options) =>
                new MockServerRouteOptions
                {
                        Path = Path,
                        Method = Method,
                        Mock = options.Mock ?? Mock,
                        Delay = options.Delay == 0 ? Delay : options.Delay,
                        Validate = options.Validate == MockServerOptionsValidationMode.Undefined
                                           ? Validate
                                           : options.Validate
                };
    }
}