using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Context.Types;

namespace ITExpert.OpenApi.Server.Core.MockServer.RequestHandlers
{
    public interface IMockServerRequestHandler
    {
        Task<MockServerResponseContext> HandleAsync(RequestContext context);
    }
}