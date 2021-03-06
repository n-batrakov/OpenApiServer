using Microsoft.Extensions.Configuration;

namespace OpenApiServer.Core.MockServer.Options
{
    public class MockServerRouteOptions
    {
        public string Path { get; set; }
        public MockServerOptionsHttpMethod Method { get; set; }

        public string Handler { get; set; }
        public IConfiguration Config { get; set; }

        public static MockServerRouteOptions Default =>
                new MockServerRouteOptions
                {
                        Path = "**",
                        Method = MockServerOptionsHttpMethod.Any,
                        Handler = "default",
                };
    }
}