using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Types;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public interface IMockServerRequestHandler
    {
        Task<IMockServerResponseContext> HandleAsync(IMockServerRequestContext context);
    }
}