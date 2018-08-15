using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("delay")]
    public class DelayHanlder : IRequestHandler
    {
        public class Options
        {
            public int Value { get; set; }
        }

        private Options Config { get; }

        public DelayHanlder(IConfiguration config)
        {
            var options = new Options();
            config.Bind(options);
            Config = options;
        }

        public async Task<ResponseContext> HandleAsync(RequestContext context)
        {
            await Task.Delay(Config.Value);
            return null;
        }
    }
}