using System;
using System.IO;

namespace ITExpert.OpenApi.DocumentProviders
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

            return DocumentSourceType.Unknown;
        }
    }
}