using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json.Linq;

using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    [RequestHandler("merge")]
    public class MergeHandler : IRequestHandler
    {
        private IConfiguration Config { get; }
        private IRequestHandlerProvider HandlerProvider { get; }

        private static readonly JsonMergeSettings MergeSettings =
                new JsonMergeSettings
                {
                        MergeArrayHandling = MergeArrayHandling.Union,
                        MergeNullValueHandling = MergeNullValueHandling.Ignore
                };

        public MergeHandler(IConfiguration config, IRequestHandlerProvider handlerProvider)
        {
            Config = config;
            HandlerProvider = handlerProvider;
        }

        public async Task<ResponseContext> HandleAsync(RequestContext requestContext)
        {
            var handlersConfigs = Config.GetSection("handlers").GetChildren();
            var responseTasks = InvokeHandlersAsync(requestContext, handlersConfigs).ToArray();
            await Task.WhenAll(responseTasks).ConfigureAwait(false);

            var responses = responseTasks.Select(x => x.Result);

            return responses.Aggregate(Merge);
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

                var handler = HandlerProvider.GetHandler(name, handlerConfig);

                yield return handler.HandleAsync(requestContext);
            }
        }

        private static ResponseContext Merge(ResponseContext target, ResponseContext source) =>
                CanMergeResponses(target, source)
                        ? source
                        : new ResponseContext
                          {
                                  BreakPipeline = target.BreakPipeline || source.BreakPipeline,
                                  ContentType = source.ContentType,
                                  StatusCode = source.StatusCode,
                                  Body = MergeJsonBodies(source.Body, target.Body),
                                  Headers = MergeHeaders(source.Headers, target.Headers)
                          };

        private static bool CanMergeResponses(ResponseContext target, ResponseContext source) =>
                source.Body != null &&
                source.ContentType == target.ContentType &&
                target.ContentType == "application/json" &&
                source.StatusCode == target.StatusCode;

        private static string MergeJsonBodies(string target, string source)
        {
            var targetJson = JToken.Parse(target);
            var sourceJson = JToken.Parse(source);

            if (targetJson.Type != sourceJson.Type)
            {
                return target;
            }

            if (targetJson is JContainer targetContainer && sourceJson is JContainer sourceContainer)
            {
                targetContainer.Merge(sourceContainer, MergeSettings);
                return targetContainer.ToString();
            }

            return target;
        }

        private static IDictionary<string, StringValues> MergeHeaders(IDictionary<string, StringValues> target,
                                                                      IDictionary<string, StringValues> source)
        {
            var result = new Dictionary<string, StringValues>(target);

            foreach (var (key, value) in source)
            {
                result[key] = value;
            }

            return result;
        }
    }
}