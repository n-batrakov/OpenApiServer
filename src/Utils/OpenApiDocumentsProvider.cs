using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Utils
{
    public static class OpenApiDocumentsProvider
    {
        private static readonly OpenApiStringReader Reader =
                new OpenApiStringReader();

        private static readonly JsonMergeSettings MergeSettings =
                new JsonMergeSettings
                {
                        MergeArrayHandling = MergeArrayHandling.Merge,
                        MergeNullValueHandling = MergeNullValueHandling.Ignore
                };

        public static IEnumerable<OpenApiDocument> GetDocuments(string directory, bool recursively = true)
        {
            var files = GetFiles(directory, new[] {"*.json", "*.yml", "*.yaml"}, recursively);

            var specs = files
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(ConvertToSpec)
                        .Where(x => x != null);

            return GroupBySpecName(specs).Select(MergeSpecs).ToArray();
        }

        private static IEnumerable<string> GetFiles(string directory, string[] patterns, bool recursively)
        {
            return patterns.SelectMany(GetPaths).Select(File.ReadAllText);

            IEnumerable<string> GetPaths(string pattern) =>
                    recursively
                            ? Directory.GetFiles(directory, pattern, SearchOption.AllDirectories)
                            : Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
        }

        private static OpenApiDocument ConvertToSpec(string spec)
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

        private static IEnumerable<IEnumerable<OpenApiDocument>> GroupBySpecName(IEnumerable<OpenApiDocument> specs) =>
                specs.OrderBy(x => x.Info.Version).GroupBy(x => x.GetId()).Select(x => (IEnumerable<OpenApiDocument>)x);

        private static OpenApiDocument MergeSpecs(IEnumerable<OpenApiDocument> doc)
        {
            var jsonSpec = doc.Select(SerializeAsJson).Aggregate(MergeJson).ToString();
            return ConvertToSpec(jsonSpec);

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