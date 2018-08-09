using System.Threading.Tasks;

using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.RequestHandlers
{
    public interface IMockServerRequestHandler
    {
        Task<MockServerResponseContext> HandleAsync(RequestContext context);
    }
}