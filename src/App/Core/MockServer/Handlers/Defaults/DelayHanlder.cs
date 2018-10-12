using System.Threading.Tasks;

using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("delay", typeof(Options))]
    public class DelayHandler : IRequestHandler
    {
        public class Options
        {
            public int Value { get; set; }
        }

        private Options Config { get; }

        public DelayHandler(Options options)
        {
            Config = options;
        }

        public async Task<ResponseContext> HandleAsync(RouteContext request)
        {
            await Task.Delay(Config.Value);
            return null;
        }
    }
}