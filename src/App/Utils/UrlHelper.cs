using System;
using System.Linq;

namespace ITExpert.OpenApi.Server.Utils
{
    internal static class UrlHelper
    {
        public static string Join(params string[] segments) =>
                string.Join("/", segments.Select(x => x == "/" ? "" : x.Trim('/')));

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