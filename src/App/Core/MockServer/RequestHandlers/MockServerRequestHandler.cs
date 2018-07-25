using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

namespace ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers
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
            var delayTask = Task.Delay(context.Options.Delay);
            var responseTask = context.Options.Mock
                                       ? MockHandler.HandleAsync(context)
                                       : ProxyHandler.HandleAsync(context);
            return Task.WhenAll(delayTask, responseTask).ContinueWith(_ => responseTask.Result);
        }
    }
}