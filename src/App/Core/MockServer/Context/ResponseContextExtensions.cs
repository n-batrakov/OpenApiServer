using System.Collections.Generic;

using Microsoft.Extensions.Primitives;

using Newtonsoft.Json.Linq;

using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Context
{
    public static class ResponseContextExtensions
    {
        private static readonly JsonMergeSettings JsonMergeSettings =
                new JsonMergeSettings
                {
                        MergeArrayHandling = MergeArrayHandling.Union,
                        MergeNullValueHandling = MergeNullValueHandling.Ignore
                };

        public static ResponseContext Merge(this ResponseContext target, ResponseContext source) =>
                CanMergeResponses(target, source)
                        ? new ResponseContext
                        {
                            BreakPipeline = target.BreakPipeline || source.BreakPipeline,
                            ContentType = source.ContentType,
                            StatusCode = source.StatusCode,
                            Body = MergeJsonBodies(source.Body, target.Body),
                            Headers = MergeHeaders(source.Headers, target.Headers)
                        }
                        : source;

        private static bool CanMergeResponses(ResponseContext target, ResponseContext source) =>
                !string.IsNullOrEmpty(source.Body) &&
                !string.IsNullOrEmpty(target.Body) &&
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
                targetContainer.Merge(sourceContainer, JsonMergeSettings);
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