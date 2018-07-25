using System.Threading.Tasks;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public interface IMockServerRequestHandler
    {
        Task<IMockServerResponseContext> HandleAsync(IMockServerRequestContext context);
    }
}