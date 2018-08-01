using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.DocumentProviders;
using ITExpert.OpenApi.Server.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Tools.Commands.Server.DocumentProviders
{
    public class DiscoveryOpenApiDocumentProvider : IOpenApiDocumentProvider
    {
        private ILogger Logger { get; }

        private IHttpClientFactory ClientFactory { get; }

        private string Key { get; }
        private string Uri { get; }

        public DiscoveryOpenApiDocumentProvider(IHttpClientFactory clientFactory,
                                                string uri,
                                                string key,
                                                ILogger logger)
        {
            Logger = logger;
            ClientFactory = clientFactory;
            Key = key;
            Uri = uri;
        }

        public IEnumerable<OpenApiDocument> GetDocuments()
        {
            var discoveryFile = LoadDiscoveryFile(Uri);
            var uris = GetUris(discoveryFile);
            var tasks = uris
                        .Select(LoadSpecsAsync)
                        .Select(x => x.ContinueWith(ConvertToSpec))
                        .ToArray();

            Task.WhenAll(tasks).Wait();

            return tasks.Select(x => x.Result).Where(x => x != null);
        }

        private JToken LoadDiscoveryFile(string uri)
        {
            var type = DocumentSourceTypeProvider.GetSourceType(uri);
            var discoveryFileText = GetContent(type);
            return JToken.Parse(discoveryFileText);

            string GetContent(DocumentSourceType source)
            {
                switch (source)
                {
                    case DocumentSourceType.File:
                        return File.ReadAllText(uri);
                    case DocumentSourceType.Directory:
                        throw new NotSupportedException(
                                $"Discovery may only be used with files and web-resources. '{uri}' is a directory.");
                    case DocumentSourceType.Web:
                        var client = ClientFactory.CreateClient();
                        return client.GetStringAsync(uri).Result;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private IEnumerable<string> GetUris(JToken discoveryFile)
        {
            if (discoveryFile.Type == JTokenType.Array)
            {
                return FromArray(discoveryFile);
            }
            if (discoveryFile.Type == JTokenType.Object)
            {
                if (string.IsNullOrEmpty(Key))
                {
                    throw new FormatException(
                            "Unable to parse discovery file. It's an object, but no '--discovery-key' specified.");
                }

                var prop = discoveryFile[Key];
                return FromArray(prop);
            }

            throw new FormatException("Unable to parse discovery file. Format is invalid.");

            IEnumerable<string> FromArray(JToken array) =>
                    array.Children()
                         .OfType<JObject>()
                         .Select(x => x.Value<string>("url"))
                         .Where(x => x != null);
        }

        private Task<string> LoadSpecsAsync(string specUrl)
        {
            var client = ClientFactory.CreateClient();
            var hasAbsoluteUri = TryGetAbsoluteSpecUri(specUrl, out var uri);
            if (!hasAbsoluteUri)
            {
                return Task.FromResult((string)null);
            }

            var response = client.GetAsync(uri);
            return response.ContinueWith(GetResponseContent).Unwrap();

            Task<string> GetResponseContent(Task<HttpResponseMessage> x)
            {
                if (x.Result.IsSuccessStatusCode)
                {
                    return x.Result.Content.ReadAsStringAsync();
                }
                else
                {
                    LogSpecLoadingError(uri.ToString(), x.Result.StatusCode);
                    return Task.FromResult((string)null);
                }
            }
        }

        private bool TryGetAbsoluteSpecUri(string url, out Uri result)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                result = uri;
                return true;
            }

            if (!string.IsNullOrEmpty(Uri))
            {
                result = url.StartsWith('/')
                                 ? new Uri(UrlHelper.Join(UrlHelper.GetHost(Uri), url), UriKind.Absolute)
                                 : new Uri(UrlHelper.Join(Uri, url), UriKind.Absolute);

                return true;
            }

            var msg =
                    $"Unable to request spec from discovery file ('{url}'). Spec URL must be either absolute or discovery file must come from web.";
            Logger.LogWarning(msg);

            result = null;
            return false;
        }

        private void LogSpecLoadingError(string uri, HttpStatusCode statusCode)
        {
            var msg = $"Unable to get spec from '{uri}'.\n      Remote server responded with {(int)statusCode} ({statusCode})";
            Logger.LogWarning(msg);
        }

        private static OpenApiDocument ConvertToSpec(Task<string> spec)
        {
            if (spec.IsCompleted && spec.Result != null)
            {
                return OpenApiDocumentUtils.ReadSpec(spec.Result);
            }
            else
            {
                return null;
            }
        }
    }
}