using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Utils
{
    public static class OpenApiDocumentUtils
    {
        private static readonly OpenApiStringReader Reader =
                new OpenApiStringReader();

        private static readonly JsonMergeSettings MergeSettings =
                new JsonMergeSettings
                {
                        MergeArrayHandling = MergeArrayHandling.Merge,
                        MergeNullValueHandling = MergeNullValueHandling.Ignore
                };

        public static OpenApiDocument ReadSpec(string spec)
        {
            try
            {
                var doc = Reader.Read(spec, out var diag);
                if (diag.Errors.Any())
                {
                    throw new AggregateException(diag.Errors.Select(x => new OpenApiFormatException(x)));
                }

                return doc;
            }
            catch (ArgumentException)
            {
                // Cannot read file. 
                // Probably spec version is not specified. 
                // Each spec must contain either `openapi: \"x.x.x\"` or `swagger: 2.0`
                return null;
            }
            catch (NullReferenceException)
            {
                throw new OpenApiFormatException("Cannot read file. Probably some refs point nowhere.");
            }
        }

        public static IEnumerable<OpenApiDocument> MergeSpecs(IEnumerable<OpenApiDocument> doc)
        {
            return GroupBySpecName(doc).Select(Merge).ToArray();


            IEnumerable<IEnumerable<OpenApiDocument>> GroupBySpecName(IEnumerable<OpenApiDocument> specs) =>
                    specs.OrderBy(x => x.Info.Version).GroupBy(x => x.GetId()).Select(x => (IEnumerable<OpenApiDocument>)x);

            OpenApiDocument Merge(IEnumerable<OpenApiDocument> specs)
            {
                var jsonSpec = specs.Select(SerializeAsJson).Aggregate(MergeJson).ToString();
                return ReadSpec(jsonSpec);
            }

            JObject SerializeAsJson(OpenApiDocument spec) =>
                    JObject.Parse(OpenApiSerializer.Serialize(spec));

            JObject MergeJson(JObject first, JObject second)
            {
                first.Merge(second, MergeSettings);
                return first;
            }
        }
    }
}