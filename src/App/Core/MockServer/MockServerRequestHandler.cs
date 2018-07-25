using System.Threading.Tasks;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public class MockServerRequestHandler : IMockServerRequestHandler
    {
        private MockRequestHandler MockHandler { get; }
        private ProxyRequestHandler ProxyHandler { get; }

        public MockServerRequestHandler(MockRequestHandler mockHandler, ProxyRequestHandler proxyHandler)
        {
            MockHandler = mockHandler;
            ProxyHandler = proxyHandler;
        }

        public Task<IMockServerResponseContext> HandleAsync(IMockServerRequestContext context)
        {
            var delayTask = Task.Delay(context.Options.Latency);
            var responseTask = context.Options.Mock
                                       ? MockHandler.HandleAsync(context)
                                       : ProxyHandler.HandleAsync(context);
            return Task.WhenAll(delayTask, responseTask).ContinueWith(_ => responseTask.Result);
        }
    }
}