using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Core.MockServer.Context.Types;
using ITExpert.OpenApi.Core.MockServer.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Core.MockServer.Context
{
    public class RequestContextProvider
    {
        private ILogger Logger { get; }
        private IDictionary<RouteId, RequestContext> Contexts { get; }

        public IReadOnlyCollection<RouteId> Routes { get; }

        public RequestContextProvider(IEnumerable<OpenApiDocument> specs,
                               IOptionsMonitor<MockServerOptions> options,
                               ILoggerFactory loggerFactory)
        {
            Contexts = RequestContextCollectionBuilder.Build(options.CurrentValue, specs);
            Routes = Contexts.Keys.ToArray();

            options.OnChange(OnOptionsChange);

            Logger = loggerFactory.CreateLogger("Context");
        }

        public RequestContext GetContext(HttpContext ctx) =>
                GetContext(ctx.GetRouteId(), ctx);

        public RequestContext GetContext(RouteId id, HttpContext ctx) => 
                Contexts[id].WithRequest(ctx, Logger);

        private void OnOptionsChange(MockServerOptions options)
        {
            RequestContextCollectionBuilder.UpdateContexts(Contexts, options);
        }
    }
}