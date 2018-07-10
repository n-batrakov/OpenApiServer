using System;
using System.Collections.Generic;
using System.Linq;

using ITExpert.OpenApi.Server.Internals;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace OpenApiServer.UnitTests.Utils
{
    public class RequestBuilder
    {
        private string Route { get; set; }
        private IHeaderDictionary Headers { get; set; }
        private IQueryCollection Query { get; set; }
        private IFormCollection Form { get; set; }
        private string Body { get; set; }

        private RequestBuilder()
        {
            Headers = new HeaderDictionary();
            Query = new QueryCollection();
            Form = new FormCollection(new Dictionary<string, StringValues>());
            Body = string.Empty;
        }

        public static RequestBuilder FromUrl(string url)
        {
            var builder = new RequestBuilder();
            ApplyRouteAndQuery(builder, url);
            return builder;
        }

        public HttpRequestValidationContext Build() =>
                new HttpRequestValidationContext
                {
                        Route = Route,
                        Query = Query,
                        Body = Body,
                        Form = Form,
                        Headers = Headers
                };

        public RequestBuilder WithBody(string body)
        {
            Body = body;
            return this;
        }

        public RequestBuilder WithHeaders(params (string Key, string Value)[] headers)
        {
            var dict = headers.ToDictionary(x => x.Key, x => new StringValues(x.Value));
            Headers = new HeaderDictionary(dict);
            return this;
        }

        public RequestBuilder WithForm(params (string Key, string Value)[] form)
        {
            var dict = form.ToDictionary(x => x.Key, x => new StringValues(x.Value));
            Form = new FormCollection(dict);
            return this;
        }


        private static void ApplyRouteAndQuery(RequestBuilder builder, string url)
        {
            var split = url.Split("?");
            builder.Route = split[0];
            if (split.Length == 1)
            {
                return;
            }

            if (split.Length == 2)
            {
                builder.Query = CreateQueryFromString(split[1]);
                return;
            }

            throw new FormatException("Invalid URI");
        }

        private static IQueryCollection CreateQueryFromString(string query)
        {
            var split = query.Split("&");
            var dict = new Dictionary<string, StringValues>();
            foreach (string queryParam in split)
            {
                if (string.IsNullOrEmpty(queryParam))
                {
                    continue;
                }

                var keyValue = queryParam.Split("=");
                var key = keyValue[0];
                var value = keyValue.Length == 2 ? keyValue[1] : null;
                dict.Add(key, value);
            }

            return new QueryCollection(dict);
        }
    }
}