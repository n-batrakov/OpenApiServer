using System;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    internal static class UrlHelper
    {
        public static string GetPathPrefix(string url)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            return GetLocalPath(uri);
        }

        public static string GetHost(string url, string defaultHost)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            return uri.IsAbsoluteUri ? uri.Host : defaultHost;
        }

        private static string GetLocalPath(Uri uri) => uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;
    }
}