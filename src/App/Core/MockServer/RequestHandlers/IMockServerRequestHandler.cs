using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

namespace ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers
{
    public interface IMockServerRequestHandler
    {
        Task<IMockServerResponseContext> HandleAsync(IMockServerRequestContext context);
    }
}