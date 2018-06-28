using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace App
{
    public static class OpenApiDocumentsProvider
    {
        private static readonly OpenApiStringReader Reader = new OpenApiStringReader();
        private static readonly JsonMergeSettings MergeSettings = new JsonMergeSettings {
            
        };
        
        public static IEnumerable<OpenApiDocument> GetDocuments(string directory)
        {
            var yamlFiles = GetFilesRecursiveley(directory, "*.yml");
            var jsonFiles = GetFilesRecursiveley(directory, "*.json");
            var specs = Enumerable.Concat(yamlFiles, jsonFiles).Select(ConvertToSpec);

            var result = GroupBySpecName(specs).Select(MergeSpecs);

            return result.ToArray();
        }

        private static IEnumerable<string> GetFilesRecursiveley(string directory, string pattern)
        {
            return GetPaths(directory).Select(File.ReadAllText);

            IEnumerable<string> GetPaths(string dir) =>
                Directory
                    .GetFiles(directory, pattern)
                    .Concat(Directory.GetDirectories(directory).SelectMany(GetPaths));

        }

        private static OpenApiDocument ConvertToSpec(string spec) => Reader.Read(spec, out var _);

        private static IEnumerable<IEnumerable<OpenApiDocument>> GroupBySpecName(IEnumerable<OpenApiDocument> specs)
        {
            throw new NotImplementedException();
        }
        
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