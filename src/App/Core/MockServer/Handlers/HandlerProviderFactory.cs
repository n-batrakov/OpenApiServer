using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace OpenApiServer.Core.MockServer.Handlers
{
    public static class HandlerProviderFactory
    {
        public static IRequestHandlerProvider CreateHandlerProvider(IServiceProvider serviceProvider, params Assembly[] assemblies)
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

            var handlerFactory = serviceProvider.GetRequiredService<RequestHandlerFactory>();

            return new RequestHandlerProvider(handlerMap, handlerFactory);

            bool IsHandlerType(Type t) => typeof(IRequestHandler).IsAssignableFrom(t);
        }
    }
}