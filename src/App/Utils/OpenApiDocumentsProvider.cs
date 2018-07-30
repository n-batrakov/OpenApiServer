using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Server.Utils
{
    public class OpenApiDocumentsProvider : IOpenApiDocumentProvider
    {
        private string RootDirectory { get; }

        public OpenApiDocumentsProvider(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public IEnumerable<OpenApiDocument> GetDocuments() => GetDocuments(RootDirectory);

        public static IEnumerable<OpenApiDocument> GetDocuments(string directory, bool recursively = true)
        {
            var files = GetFiles(directory, new[] {"*.json", "*.yml", "*.yaml"}, recursively);

            var specs = files
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(OpenApiDocumentUtils.ReadSpec)
                        .Where(x => x != null);

            return OpenApiDocumentUtils.MergeSpecs(specs).ToArray();
        }

        private static IEnumerable<string> GetFiles(string directory, string[] patterns, bool recursively)
        {
            return patterns.SelectMany(GetPaths).Select(File.ReadAllText);

            IEnumerable<string> GetPaths(string pattern) =>
                    recursively
                            ? Directory.GetFiles(directory, pattern, SearchOption.AllDirectories)
                            : Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
        }
    }
}