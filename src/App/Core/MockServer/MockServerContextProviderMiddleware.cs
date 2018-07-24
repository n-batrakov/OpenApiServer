using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ITExpert.OpenApi.Server.Core.MockServer.Options;
using ITExpert.OpenApi.Server.Core.MockServer.Validation;
using ITExpert.OpenApi.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    public interface IOpenApiOperationPathProvider
    {
        string GetPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath);
    }

    public class DefaultOperationPathProvider : IOpenApiOperationPathProvider
    {
        public string GetPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath)
        {
            return GetDefaultPath(spec, operation, operationPath);
        }

        internal static string GetDefaultPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath)
        {
            var service = spec.Info.Title.Replace(" ", "");
            var path = operationPath;
            var version = $"/{spec.Info.GetMajorVersion()}";
            return $"{service}{version}{path}";
        }
    }

    public class ConfigOperationPathProvider : IOpenApiOperationPathProvider
    {
        private string PathFormatString { get; }

        private bool UseDefault { get; }

        public ConfigOperationPathProvider(IOptions<MockServerOptions> options)
        {
            PathFormatString = options.Value.Route?.ToLowerInvariant();
            UseDefault = string.IsNullOrEmpty(options.Value.Route);
        }

        public string GetPath(OpenApiDocument spec, OpenApiOperation operation, string operationPath)
        {
            if (UseDefault)
            {
                return GetDefaultValue(spec, operation, operationPath);
            }

            var service = spec.Info.Title.Replace(" ", "");
            var path = operationPath.Skip(1);
            var version = spec.Info.GetMajorVersion();

            return Format(PathFormatString, ("service", service), ("path", path), ("version", version));
        }

        private static string GetDefaultValue(OpenApiDocument spec, OpenApiOperation operation, string operationPath) =>
                DefaultOperationPathProvider.GetDefaultPath(spec, operation, operationPath);

        private static string Format(string str, params (string, object)[] args)
        {
            foreach (var (name, value) in args)
            {
                str = str.Replace($"{{{name}}}", value.ToString());
            }

            return str;
        }
    }


    public class MockServerContextProviderMiddleware
    {
        
    }





    public class MockServerRequestContextProvider
    {
        private IOpenApiOperationPathProvider PathProvider { get; }

        private IDictionary<(string, string), OpenApiOperation> AvailableRoutes { get; }
        private IDictionary<(string, string), MockServerRouteOptions> RouteConfigs { get; set; }

        public MockServerRequestContextProvider(IOpenApiOperationPathProvider pathProvider,
                                                IOptionsMonitor<MockServerOptions> options,
                                                IEnumerable<OpenApiDocument> specs)
        {
            PathProvider = pathProvider;

            options.OnChange(OnOptionsChange);
            
            AvailableRoutes = GetAvailableRoutes(specs);
            RouteConfigs = UnfoldConfig(options.CurrentValue, AvailableRoutes);
        }

        private void OnOptionsChange(MockServerOptions options)
        {
            RouteConfigs = UnfoldConfig(options, AvailableRoutes);
        }

        public IMockServerRequestContext GetContext(HttpContext ctx)
        {
            var path = ctx.Request.Path.ToString().ToLowerInvariant();
            var verb = ctx.Request.Method.ToLowerInvariant();
            var key = (path, verb);
            var spec = AvailableRoutes[key];
            var config = RouteConfigs[key];
            return CreateRequestContext(ctx, spec, config);
        }

        public IEnumerable<(string Path, string Verb)> Routes => AvailableRoutes.Keys;

        private static IDictionary<(string, string), MockServerRouteOptions> UnfoldConfig(
                MockServerOptions options,
                IDictionary<(string, string), OpenApiOperation> availableRoutes)
        {
            return new Dictionary<(string, string), MockServerRouteOptions>(Generator());

            IEnumerable<KeyValuePair<(string, string), MockServerRouteOptions>> Generator()
            {
                foreach (var routeConfig in options.Routes)
                {
                    var regexp = new Regex(routeConfig.Path);
                    var matchedRoutes = availableRoutes.Keys.Where(x => regexp.IsMatch(x.Item1));
                    foreach (var (route, method) in matchedRoutes)
                    {
                        if (!IsMatch(method, routeConfig.Method))
                        {
                            continue;
                        }

                        yield return new KeyValuePair<(string, string), MockServerRouteOptions>(
                                (route, method),
                                routeConfig);
                    }
                }
            }
        }

        private static bool IsMatch(string specMethod, MockServerOptionsHttpMethod configMethod)
        {
            throw new NotImplementedException();
        }

        private IDictionary<(string, string), OpenApiOperation> GetAvailableRoutes(IEnumerable<OpenApiDocument> specs)
        {
            return new Dictionary<(string, string), OpenApiOperation>(Generator());

            IEnumerable<KeyValuePair<(string, string), OpenApiOperation>> Generator()
            {
                foreach (var spec in specs)
                {
                    foreach (var (path, routeSpec) in spec.Paths)
                    {
                        foreach (var (verb, operation) in routeSpec.Operations)
                        {
                            var route = PathProvider.GetPath(spec, operation, path).ToLowerInvariant();
                            var method = verb.ToString().ToLowerInvariant();
                            var value = operation;
                            yield return new KeyValuePair<(string, string), OpenApiOperation>((route, method), value);
                        }
                    }
                }
            }
        }

        private static IMockServerRequestContext CreateRequestContext(HttpContext ctx,
                                                               OpenApiOperation operation,
                                                               MockServerRouteOptions routeOptions)
        {
            return new MockServerRequestContext
                   {
                           Path = ctx.Request.Path,
                           ContentType = ctx.Request.ContentType,
                           Query = ctx.Request.Query,
                           Headers = ctx.Request.Headers,
                           Route = ctx.GetRouteData(),
                           Body = ctx.Request.HasFormContentType ? ReadForm() : ReadBody(),
                           Options = routeOptions,
                           OperationSpec = operation
                   };

            string ReadForm()
            {
                var dict = ctx.Request.Form.ToDictionary(x => x.Key, x => JToken.Parse(x.Value));
                return JsonConvert.SerializeObject(dict);
            }

            string ReadBody()
            {
                using (var reader = new StreamReader(ctx.Request.Body))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private class MockServerRequestContext : IMockServerRequestContext
        {
            public string Path { get; set; }

            public RouteData Route { get; set; }

            public IHeaderDictionary Headers { get; set; }

            public IQueryCollection Query { get; set; }

            public string Body { get; set; }

            public string ContentType { get; set; }

            public OpenApiOperation OperationSpec { get; set; }

            public MockServerRouteOptions Options {get; set; }
        }
    }

    public class MockServerBuilder
    {
        private IEnumerable<OpenApiDocument> Specs { get; }
        private IOpenApiOperationPathProvider PathProvider { get; }

        private MockServerRequestContextProvider ContextProvider { get; }

        

        public MockServerBuilder(IApplicationBuilder app)
        {
        }

        private IRouter BuildRouter(IRouteBuilder builder)
        {
            foreach (var (path, verb) in ContextProvider.Routes)
            {
                builder.MapVerb(verb, path, HandleRequest);
            }

            return builder.Build();
        }

        private Task HandleRequest(HttpContext ctx)
        {
            var requestContext = ContextProvider.GetContext(ctx);
            var config = requestContext.Options;
            ctx.Features.Set(requestContext);

            if (config.Validate == MockServerOptionsValidationMode.All ||
                config.Validate == MockServerOptionsValidationMode.Request)
            {
                //TODO: Validate Request
            }

            if (config.Mock)
            {
                // TODO: Call mock handler
            }
            else
            {
                // TODO: Call proxy handler
            }

            if (config.Validate == MockServerOptionsValidationMode.All ||
                config.Validate == MockServerOptionsValidationMode.Response)
            {
                //TODO: Validate Response
            }

            throw new NotImplementedException();
        }



        
    }

    public interface IMockServerRequestContext
    {
        string Path { get; }
        RouteData Route { get; }
        IHeaderDictionary Headers { get; }
        IQueryCollection Query { get; }
        string Body { get; }
        string ContentType { get; }
        OpenApiOperation OperationSpec { get; }
        MockServerRouteOptions Options { get; }
    }

    public interface IMockServerResponseContext
    {
        HttpStatusCode StatusCode { get; }
        Stream Body { get; }
        IDictionary<string, StringValues> Headers { get; }
    }

    public interface IMockRequestHandler
    {
        Task<IMockServerResponseContext> HandleAsync(IMockServerRequestContext context);
    }
}