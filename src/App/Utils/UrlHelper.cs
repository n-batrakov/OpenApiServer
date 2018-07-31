using System;
using System.Linq;

namespace ITExpert.OpenApi.Server.Utils
{
    public static class UrlHelper
    {
        public static string Join(params string[] segments)
        {
            var sanitizedSegments = segments
                                    .Where(x => !string.IsNullOrEmpty(x))
                                    .Select(x => x == "/" ? "" : x.Trim('/'));
            return string.Join("/", sanitizedSegments);
        }

        public static string GetDefaultPathPrefix(string service) => $"api/{service}";

        public static string GetPathPrefix(string url)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            var prefix = GetLocalPath(uri);
            return Sanitize(prefix);
        }

        public static string GetHost(string url, string defaultHost = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                return defaultHost;
            }

            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            return uri.IsAbsoluteUri ? uri.Host : defaultHost;
        }

        private static string GetLocalPath(Uri uri) => uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

        private static string Sanitize(string url)
        {
            return url.Replace("//", "/");
        }
    }
}