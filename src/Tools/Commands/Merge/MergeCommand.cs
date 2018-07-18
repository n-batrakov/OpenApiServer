using System;
using System.Collections.Generic;
using System.IO;

using ITExpert.OpenApi.Utils;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace ITExpert.OpenApi.Tools.Commands.Merge
{
    public class MergeCommand
    {
        private bool FlatOutput { get; }
        private bool GetFilesRecursivly { get; }
        private string OutputDirectory { get; }
        private string RootDirectory { get; }
        private OpenApiFormat OutputFormat { get; }
        private OpenApiSpecVersion OutputVersion { get; }

        public MergeCommand(MergeOptions options)
        {
            OutputDirectory = options.Output ?? "out";
            RootDirectory = options.Root ?? ".";
            GetFilesRecursivly = options.Recursive;
            FlatOutput = options.Flat;

            if (string.IsNullOrEmpty(options.Format))
            {
                OutputFormat = OpenApiFormat.Json;
            }
            else
            {
                var formatParsed = Enum.TryParse(options.Format, out OpenApiFormat format);
                OutputFormat = formatParsed ? format : OpenApiFormat.Json;
            }

            if (string.IsNullOrEmpty(options.Version))
            {
                OutputVersion = OpenApiSpecVersion.OpenApi3_0;
            }
            else
            {
                OutputVersion = options.Version == "2" ? OpenApiSpecVersion.OpenApi2_0 : OpenApiSpecVersion.OpenApi3_0;
            }
        }

        public int Execute()
        {
            var docs = OpenApiDocumentsProvider.GetDocuments(RootDirectory, GetFilesRecursivly);
            WriteSpecs(docs);
            return 0;
        }

        private void WriteSpecs(IEnumerable<OpenApiDocument> specs)
        {
            foreach (var spec in specs)
            {
                WriteOneSpec(spec);
            }

            void WriteOneSpec(OpenApiDocument spec)
            {
                var path = FlatOutput
                                   ? GetSpecFilePathFlat(spec.Info.GetMajorVersion(), spec.Info.Title)
                                   : GetSpecFilePath(spec.Info.GetMajorVersion(), spec.Info.Title);
                var content = OpenApiSerializer.Serialize(spec, OutputVersion, OutputFormat);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var writer = File.CreateText(path))
                {
                    writer.Write(content);
                }
            }

            string GetSpecFilePath(string apiVersion, string apiTitle)
            {
                var filename = $"openapi.{OutputExtension}";
                var ver = $"v{apiVersion}";
                var name = apiTitle.Replace(" ", "");
                return Path.Combine(OutputDirectory, name, ver, filename).ToLowerInvariant();
            }

            string GetSpecFilePathFlat(string apiVersion, string apiTitle)
            {
                var title = apiTitle.Replace(" ", "");
                var filename = $"{title}@{apiVersion}.{OutputExtension}";
                return Path.Combine(OutputDirectory, filename);
            }
        }

        private string OutputExtension
        {
            get
            {
                switch (OutputFormat)
                {
                    case OpenApiFormat.Json:
                        return "json";
                    case OpenApiFormat.Yaml:
                        return "yml";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}