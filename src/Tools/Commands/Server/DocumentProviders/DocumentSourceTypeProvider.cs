using System;
using System.IO;

namespace ITExpert.OpenApi.Tools.Commands.Server.DocumentProviders
{
    public static class DocumentSourceTypeProvider
    {
        public static DocumentSourceType GetSourceType(string uri)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;
            if (uri.StartsWith("http://", comparison) || uri.StartsWith("https://", comparison))
            {
                return DocumentSourceType.Web;
            }

            if (Directory.Exists(uri))
            {
                return DocumentSourceType.Directory;
            }

            if (File.Exists(uri))
            {
                return DocumentSourceType.File;
            }

            throw new ArgumentException(
                    $"Unable to determine type for given URI ('{uri}'). " +
                    "If it is file or directory, make sure it exists. " +
                    "In case of web URI make sure protocol specified " +
                    "correctly (only http(-s) is supported).");
        }
    }
}