using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;

using OpenApiServer.Core.MockServer.Context.Types;
using OpenApiServer.Core.MockServer.Exceptions;
using OpenApiServer.Core.MockServer.Handlers.Internals;

namespace OpenApiServer.Core.MockServer.Handlers
{
    public class RequestHandlerProvider
    {
        private IDictionary<string, Type> Source { get; }
        private IServiceProvider ServiceProvider { get; }

        public RequestHandlerProvider(IDictionary<string, Type> source, IServiceProvider serviceProvider)
        {
            Source = source;
            ServiceProvider = serviceProvider;
        }

        public IRequestHandler GetHandler(string id, IConfiguration handlerConfig, ResponseContext responseContext)
        {
            Source.TryGetValue(id, out var handlerType);
            if (handlerType == null)
            {
                throw new HandlerNotFoundException(id);
            }
            if (!typeof(IRequestHandler).IsAssignableFrom(handlerType))
            {
                throw new MockServerException($"Registered '{id}' handler it does not implement IRequestHandler.");
            }

            object options = null;

            var attr = handlerType.GetCustomAttribute<RequestHandlerAttribute>();
            if (handlerConfig != null && attr?.Options != null)
            {
                var optionsType = attr.Options;
                options = Activator.CreateInstance(optionsType);
                handlerConfig.Bind(options);
            }

            return RequestHandlerActivator.CreateHandler(ServiceProvider,
                                                         handlerType,
                                                         new[] {options, responseContext});
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

            return new RequestHandlerProvider(handlerMap, serviceProvider);

            bool IsHandlerType(Type t) => typeof(IRequestHandler).IsAssignableFrom(t);
        }
    }
}