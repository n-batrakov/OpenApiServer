using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using OpenApiServer.Core.MockServer.Context.Mapping;
using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Context.Types.Spec;
using OpenApiServer.Core.MockServer.Handlers.Defaults;
using OpenApiServer.Core.MockServer.MockDataProviders;
using OpenApiServer.Core.MockServer.Options;

using RouteContext = OpenApiServer.Core.MockServer.Context.Types.RouteContext;

namespace UnitTests.Utils
{
    public class RouteContextBuilder
    {
        private string Path { get; set; }
        private HttpMethod Method { get; set; }
        private RouteData Route { get; set; }
        private IHeaderDictionary Headers { get; set; }
        private IQueryCollection Query { get; set; }
        private IFormCollection Form { get; set; }
        private string Body { get; set; }
        private RouteSpec Spec { get; set; }

        private RouteContextBuilder()
        {
            Headers = new HeaderDictionary();
            Query = new QueryCollection();
            Form = null;
            Body = null;
        }

        public static RouteContextBuilder FromUrl(string url, HttpMethod method = HttpMethod.Get)
        {
            var builder = new RouteContextBuilder();
            ApplyRouteAndQuery(builder, url);
            builder.Path = url;
            builder.Method = method;
            return builder;
        }

        public RouteContext Build()
        {
            var config = new MockServerRouteOptions();
            
            var callCtx = new RequestContext
                          {
                                  Route = Route,
                                  Method = Method,
                                  Query = Query,
                                  Body = Body == null ? null : JToken.Parse(Body),
                                  Headers = Headers,
                                  ContentType = Form == null ? "application/json" : "multipart/form-data",
                                  PathAndQuery = Path,
                          };

            var handler = new MockHandler(new MockDataProvider());

            return new RouteContext(config, Spec, callCtx, handler, NullLogger.Instance);
        }

        public RouteContextBuilder WithSpec(OpenApiOperation spec)
        {
            var server = new Microsoft.OpenApi.Models.OpenApiServer[0];
            Spec = OpenApiDocumentConverter.ConvertSpec(spec, server);
            return this;
        }

        public RouteContextBuilder WithSpec(RouteSpec spec)
        {
            Spec = spec;
            return this;
        }

        public RouteContextBuilder WithBody(string body)
        {
            Body = body;
            return this;
        }

        public RouteContextBuilder WithHeaders(params (string Key, string Value)[] headers)
        {
            var dict = headers.ToDictionary(x => x.Key, x => new StringValues(x.Value));
            Headers = new HeaderDictionary(dict);
            return this;
        }

        public RouteContextBuilder WithForm(params (string Key, string Value)[] form)
        {
            var dict = form.ToDictionary(x => x.Key, x => new StringValues(x.Value));
            Form = new FormCollection(dict);
            return this;
        }


        private static void ApplyRouteAndQuery(RouteContextBuilder builder, string url)
        {
            var split = url.Split("?");
            builder.Route = new RouteData();
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