using System.Threading.Tasks;

using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Handlers
{
    public interface IRequestHandler
    {
        Task<ResponseContext> HandleAsync(RouteContext request);
    }
}