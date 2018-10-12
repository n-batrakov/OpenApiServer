using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Handlers.Internals;

namespace OpenApiServer.Core.MockServer.Handlers
{
    public class RequestHandlerProvider
    {
        private IDictionary<string, Type> Source { get; }
        private RequestHandlerActivator HandlerActivator { get; }

        public RequestHandlerProvider(IDictionary<string, Type> source, RequestHandlerActivator handlerActivator)
        {
            Source = source;
            HandlerActivator = handlerActivator;
        }

        public IRequestHandler GetHandler(string id, IConfiguration handlerConfig, ResponseContext responseContext)
        {
            Source.TryGetValue(id, out var handlerType);
            if (handlerType == null)
            {
                throw new Exception($"Unable to find handler with name '{id}'.");
            }
            if (!typeof(IRequestHandler).IsAssignableFrom(handlerType))
            {
                throw new Exception($"Registered '{id}' handler it does not implement IRequestHandler.");
            }

            object options = null;

            var attr = handlerType.GetCustomAttribute<RequestHandlerAttribute>();
            if (attr?.Options != null)
            {
                var optionsType = attr.Options;
                options = Activator.CreateInstance(optionsType);
                handlerConfig.Bind(options);
            }

            return HandlerActivator.CreateHandler(handlerType, new [] {options, responseContext});
        }

        public static RequestHandlerProvider FromAssemblies(IServiceProvider serviceProvider, params Assembly[] assemblies)
        {
            var handlerMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            var types = assemblies.SelectMany(x => x.GetExportedTypes()).Where(IsHandlerType);

            foreach (var type in types)
            {
                var key = type.GetCustomAttribute<RequestHandlerAttribute>()?.HandlerId;
                if (key == null)
                {
                    continue;
                }

                if (handlerMap.ContainsKey(key))
                {
                    var existingHandler = handlerMap[key];
                    throw new ArgumentException($"Handlers with duplicate ID found - '{existingHandler}' and '{type}'");
                }

                handlerMap.Add(key, type);
            }

            var handlerFactory = serviceProvider.GetRequiredService<RequestHandlerActivator>();

            return new RequestHandlerProvider(handlerMap, handlerFactory);

            bool IsHandlerType(Type t) => typeof(IRequestHandler).IsAssignableFrom(t);
        }
    }
}