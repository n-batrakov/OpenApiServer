using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using OpenApiServer.Core.MockServer.Context;
using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("merge")]
    public class MergeHandler : IRequestHandler
    {
        private IConfiguration Config { get; }
        private IRequestHandlerProvider HandlerProvider { get; }

        public MergeHandler(IConfiguration config, IRequestHandlerProvider handlerProvider)
        {
            Config = config;
            HandlerProvider = handlerProvider;
        }

        public async Task<ResponseContext> HandleAsync(RequestContext request)
        {
            var handlersConfigs = Config.GetSection("handlers").GetChildren();
            var responseTasks = InvokeHandlersAsync(request, handlersConfigs).ToArray();
            await Task.WhenAll(responseTasks).ConfigureAwait(false);

            var responses = responseTasks.Select(x => x.Result);

            return responses.Aggregate(ResponseContextExtensions.Merge);
        }

        private IEnumerable<Task<ResponseContext>> InvokeHandlersAsync(RequestContext requestContext,
                                                                       IEnumerable<IConfiguration> handlersConfigs)
        {
            foreach (var handlerConfig in handlersConfigs)
            {
                if (handlerConfig.GetValue("disable", false))
                {
                    continue;
                }

                var name = handlerConfig.GetValue<string>("handler");
                if (name == null)
                {
                    throw new Exception("Unable to find handler id for one of the pipeline handlers in configuration.");
                }

                var handler = HandlerProvider.GetHandler(name, handlerConfig, responseContext: null);

                yield return handler.HandleAsync(requestContext);
            }
        }
    }
}