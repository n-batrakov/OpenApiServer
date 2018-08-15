using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.ExampleProviders.Internals;
using OpenApiServer.Core.MockServer.Handlers;
using OpenApiServer.Core.MockServer.Options;
using OpenApiServer.Server.Logging;

namespace OpenApiServer.Core.MockServer.Context
{
    public class RequestContextProvider
    {
        private ILogger Logger { get; }
        private IConfiguration Config { get; }
        private IRequestHandlerProvider HandlerProvider { get; }

        private IDictionary<RouteId, RequestContext> Contexts { get; }

        public IReadOnlyCollection<RouteId> Routes { get; }

        public RequestContextProvider(IEnumerable<OpenApiDocument> specs,
                                      IConfiguration config,
                                      IRequestHandlerProvider handlerProvider,
                                      ILoggerFactory loggerFactory)
        {
            Config = config;
            Logger = loggerFactory.CreateOpenApiLogger();
            HandlerProvider = handlerProvider;

            var options = BindOptions(config);
            Contexts = RequestContextCollectionBuilder.Build(options, specs);

            Routes = Contexts.Keys.ToArray();

            var token = config.GetReloadToken();
            if (token.ActiveChangeCallbacks)
            {
                token.RegisterChangeCallback(OnConfigChange, null);
            }
        }

        public RequestContext GetContext(HttpContext ctx) =>
                GetContext(ctx.GetRouteId(), ctx);

        public RequestContext GetContext(RouteId id, HttpContext ctx)
        {
            var requestCtx = Contexts[id];
            var callCtx = GetCallContext(ctx);
            var handler = HandlerProvider.GetHandler(requestCtx.Config.Handler, requestCtx.Config.Config);
            return requestCtx.WithRequest(callCtx, handler, Logger);
        }

        private void OnConfigChange(object state)
        {
            var options = BindOptions(Config);
            OnOptionsChange(options);
        }

        private void OnOptionsChange(MockServerOptions options)
        {
            RequestContextCollectionBuilder.UpdateContexts(Contexts, options);
            Logger.LogInformation("Configuration reloaded.");
        }

        private static MockServerOptions BindOptions(IConfiguration config)
        {
            return new MockServerOptions
                   {
                           MockServerHost = config["mockServerHost"],
                           Routes = config.GetSection("routes").GetChildren().Select(GetRouteOptions).ToArray()
                   };

            MockServerRouteOptions GetRouteOptions(IConfiguration routeConfig) =>
                    new MockServerRouteOptions
                    {
                            Path = routeConfig["path"],
                            Method = GetEnumValue<MockServerOptionsHttpMethod>(routeConfig, "method"),

                            Handler = routeConfig["handler"],
                            Config = routeConfig
                    };
        }

        private static T GetEnumValue<T>(IConfiguration configuration, string key) where T : struct
        {
            var str = configuration[key];
            Enum.TryParse<T>(str, ignoreCase: true, out var result);
            return result;
        }


        private static RequestContextCall GetCallContext(HttpContext ctx)
        {
            return new RequestContextCall
            {
                PathAndQuery = ctx.Request.GetEncodedPathAndQuery(),
                Method = Enum.Parse<HttpMethod>(ctx.Request.Method, ignoreCase: true),

                ContentType = GetContentType(ctx.Request),
                Query = ctx.Request.Query,
                Headers = ctx.Request.Headers,
                Route = ctx.GetRouteData(),
                Body = GetBody(ctx.Request),
            };
        }

        private static JToken GetBody(HttpRequest request)
        {
            return request.HasFormContentType ? ReadForm() : ReadBody();

            JToken ReadForm()
            {
                var obj = new JObject();
                foreach (var (key, value) in request.Form)
                {
                    var propertyValue = ParseRawValue(value);
                    obj.Add(key, propertyValue);
                }

                var fileProperties = request.Form.Files.ToDictionary(x => x.Name, GetFilePropertyValue);
                obj.AddRange(fileProperties, overwriteDuplicateKeys: false);

                return obj;
            }

            JToken ReadBody()
            {
                using (var reader = new StreamReader(request.Body))
                {
                    var body = reader.ReadToEnd();
                    return ParseRawValue(body);
                }
            }
        }

        private static JToken GetFilePropertyValue(IFormFile file)
        {
            using (var memorySteam = new MemoryStream())
            {
                file.CopyTo(memorySteam);
                var bytes = memorySteam.ToArray();
                var base64 = Convert.ToBase64String(bytes);
                return new JValue(base64);
            }
        }

        private static string GetContentType(HttpRequest request)
        {
            var hasContentType = request.Headers.TryGetValue("Content-Type", out var values);
            if (!hasContentType || values.Count == 0)
            {
                return null;
            }

            var contentType = values.First();
            return contentType.Split(';').First();
        }

        private static JToken ParseRawValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.StartsWith('{') || value.StartsWith('['))
            {
                return JToken.Parse(value);
            }

            if (bool.TryParse(value, out var boolean))
            {
                return new JValue(boolean);
            }

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            {
                return new JValue(num);
            }

            return new JValue(value);
        }
    }
}