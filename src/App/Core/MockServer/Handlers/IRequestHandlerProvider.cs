using Microsoft.Extensions.Configuration;
using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Handlers
{
    public interface IRequestHandlerProvider
    {
        IRequestHandler GetHandler(string id, IConfiguration handlerConfig, ResponseContext responseContext);
    }
}