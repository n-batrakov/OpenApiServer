using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;

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

        public static IEnumerable<OpenApiDocument> GetDocuments(string directory)
        {
            var yamlFiles = GetFilesRecursiveley(directory, "*.yml");
            var jsonFiles = GetFilesRecursiveley(directory, "*.json");
            var files = jsonFiles.Concat(yamlFiles).ToArray();

            var specs = files
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(ConvertToSpec);

            return GroupBySpecName(specs).Select(MergeSpecs).ToArray();
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
                specs.OrderBy(x => x.Info.Version).GroupBy(x => x.GetId()).Select(x => (IEnumerable<OpenApiDocument>)x);

        private static OpenApiDocument MergeSpecs(IEnumerable<OpenApiDocument> doc)
        {
            var jsonSpec = doc.Select(SerializeAsJson).Aggregate(MergeJson).ToString();
            return ConvertToSpec(jsonSpec);

            JObject SerializeAsJson(OpenApiDocument spec)
            {
                var stringWriter = new StringWriter();
                var openApiWriter = new MyOpenApiJsonWriter(stringWriter);
                spec.Serialize(openApiWriter, OpenApiSpecVersion.OpenApi3_0);
                var json = stringWriter.ToString();
                return JObject.Parse(json);
            }

            JObject MergeJson(JObject first, JObject second)
            {
                first.Merge(second, MergeSettings);
                return first;
            }
        }

        private class MyOpenApiJsonWriter : OpenApiJsonWriter
        {
            public MyOpenApiJsonWriter(TextWriter textWriter): base(textWriter)
            {
            }

            public override void WriteValue(DateTimeOffset value) 
            {
                WriteValue(value.ToString("o"));
            }
        }
    }
}