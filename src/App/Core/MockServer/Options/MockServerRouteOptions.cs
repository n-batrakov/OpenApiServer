namespace ITExpert.OpenApi.Server.Core.MockServer.Options
{
    public static class MockServerRouteOptionsExtensions
    {
        public static bool ShouldValidateRequest(this MockServerRouteOptions x) =>
                x.Validate == MockServerOptionsValidationMode.All ||
                x.Validate == MockServerOptionsValidationMode.Request;

        public static bool ShouldValidateResponse(this MockServerRouteOptions x) =>
                x.Validate == MockServerOptionsValidationMode.All ||
                x.Validate == MockServerOptionsValidationMode.Response;
    }

    public class MockServerRouteOptions
    {
        public string Path { get; set; }
        public MockServerOptionsHttpMethod Method { get; set; }
        public bool? Mock { get; set; }

        public MockServerOptionsValidationMode Validate { get; set; }
        public ushort Delay { get; set; }
        public string Host { get; set; }

        public static MockServerRouteOptions Default =>
                new MockServerRouteOptions
                {
                        Path = ".*",
                        Method = MockServerOptionsHttpMethod.Any,
                        Delay = 0,
                        Mock = false,
                        Validate = MockServerOptionsValidationMode.None
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