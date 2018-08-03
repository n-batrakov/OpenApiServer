using System.Collections.Generic;
using System.IO;
using System.Linq;

using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.DocumentProviders
{
    public class DirectoryOpenApiDocumentsProvider : IOpenApiDocumentProvider
    {
        private string RootDirectory { get; }
        private bool Recursively { get; }
        private bool MergeSpecs { get; }

        public DirectoryOpenApiDocumentsProvider(string rootDirectory, bool recursively = true, bool mergeSpecs = true)
        {
            RootDirectory = rootDirectory;
            Recursively = recursively;
            MergeSpecs = mergeSpecs;
        }

        public IEnumerable<OpenApiDocument> GetDocuments()
        {
            var files = GetFiles(RootDirectory, new[] { "*.json", "*.yml", "*.yaml" }, Recursively);

            var specs = files
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(OpenApiDocumentUtils.ReadSpec)
                        .Where(x => x != null);

            return MergeSpecs
                           ? OpenApiDocumentUtils.MergeSpecs(specs).ToArray()
                           : specs.ToArray();
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