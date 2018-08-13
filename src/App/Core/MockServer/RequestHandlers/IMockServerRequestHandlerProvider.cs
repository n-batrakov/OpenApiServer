namespace OpenApiServer.Core.MockServer.RequestHandlers
{
    public interface IMockServerRequestHandlerProvider
    {
        IMockServerRequestHandler GetHandler(string id);
    }
}