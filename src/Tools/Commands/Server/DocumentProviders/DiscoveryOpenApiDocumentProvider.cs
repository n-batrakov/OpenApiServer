using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Utils;

using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Tools.Commands.Server.DocumentProviders
{
    public class DiscoveryOpenApiDocumentProvider : IOpenApiDocumentProvider
    {
        private IHttpClientFactory ClientFactory { get; }

        private string Key { get; }
        private string Uri { get; }

        public DiscoveryOpenApiDocumentProvider(IHttpClientFactory clientFactory, string uri, string key)
        {
            ClientFactory = clientFactory;
            Key = key;
            Uri = uri;
        }

        public IEnumerable<OpenApiDocument> GetDocuments()
        {
            var discoveryFile = LoadDiscoveryFile();
            var uris = GetUris(discoveryFile);
            var tasks = uris
                        .Select(LoadSpecsAsync)
                        .Select(x => x.ContinueWith(ConvertToSpec))
                        .ToArray();

            Task.WhenAll(tasks).Wait();

            return tasks.Select(x => x.Result);
        }

        private JToken LoadDiscoveryFile()
        {
            throw new NotImplementedException();
        }

        private IEnumerable<string> GetUris(JToken discoveryFile)
        {
            if (discoveryFile.Type == JTokenType.Array)
            {
                return discoveryFile.Values().Select(GetUrl);
            }
            else if (discoveryFile.Type == JTokenType.Object)
            {
                if (string.IsNullOrEmpty(Key))
                {
                    throw new FormatException(
                            "Unable to parse discovery file. It's an object, but no '--discovery-key' specified.");
                }
                return discoveryFile[Key].Values().Select(GetUrl);
            }

            throw new FormatException("Unable to parse discovery file. Format is invalid.");

            string GetUrl(JToken obj)
            {
                if (obj.Type != JTokenType.Object)
                {
                    throw new FormatException("Unable to parse discovery file. Format is invalid.");
                }

                return obj["url"].Value<string>();
            }
        }

        private Task<string> LoadSpecsAsync(string uri)
        {
            var client = ClientFactory.CreateClient();
            return client.GetStringAsync(uri);
        }

        private OpenApiDocument ConvertToSpec(Task<string> spec)
        {
            return OpenApiDocumentUtils.ReadSpec(spec.Result);
        }
    }
}