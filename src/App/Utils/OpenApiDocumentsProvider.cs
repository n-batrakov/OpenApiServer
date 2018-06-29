using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApiServer.Exceptions;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApiServer.Utils
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

        public static IEnumerable<OpenApiDocument> GetDocuments(string directory)
        {
            var yamlFiles = GetFilesRecursiveley(directory, "*.yml");
            var jsonFiles = GetFilesRecursiveley(directory, "*.json");
            var specs = Enumerable
                        .Concat(yamlFiles, jsonFiles)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(ConvertToSpec);

            var result = GroupBySpecName(specs).Select(MergeSpecs);

            return result.ToArray();
        }

        private static IEnumerable<string> GetFilesRecursiveley(string directory, string pattern)
        {
            return GetPaths(directory).Select(File.ReadAllText);

            IEnumerable<string> GetPaths(string dir) =>
                    Directory
                            .GetFiles(dir, pattern)
                            .Concat(Directory.GetDirectories(dir).SelectMany(GetPaths));
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
                throw new OpenApiFormatException(
                        "Cannot read file. Probably spec version is not specified. " +
                        "Each spec must contain either `openapi: \"x.x.x\"` or `swagger: 2.0`");
            }
            catch (NullReferenceException)
            {
                throw new OpenApiFormatException("Cannot read file. Probably some refs point nowhere.");
            }
        }

        private static IEnumerable<IEnumerable<OpenApiDocument>> GroupBySpecName(IEnumerable<OpenApiDocument> specs) =>
                specs.GroupBy(x => $"{x.Info.Title}:{x.Info.Version}").Select(x => (IEnumerable<OpenApiDocument>)x);

        private static OpenApiDocument MergeSpecs(IEnumerable<OpenApiDocument> doc)
        {
            var jsonSpec = doc.Select(SerializeAsJson).Aggregate(MergeJson).ToString();
            return ConvertToSpec(jsonSpec);

            JObject SerializeAsJson(OpenApiDocument spec)
            {
                var json = spec.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
                return JObject.Parse(json);
            }

            JObject MergeJson(JObject first, JObject second)
            {
                first.Merge(second, MergeSettings);
                return first;
            }
        }
    }
}