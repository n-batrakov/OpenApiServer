namespace OpenApiServer.Core.MockServer.Handlers
{
    public interface IRequestHandlerProvider
    {
        IRequestHandler GetHandler(string id);
    }
}