using System.Threading.Tasks;

using ITExpert.OpenApi.Core.MockServer.Context.Types;

namespace ITExpert.OpenApi.Core.MockServer.RequestHandlers
{
    public interface IMockServerRequestHandler
    {
        Task<MockServerResponseContext> HandleAsync(RequestContext context);
    }
}