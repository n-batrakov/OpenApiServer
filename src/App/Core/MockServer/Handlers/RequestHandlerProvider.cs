using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Extensions.Configuration;

using OpenApiServer.Core.MockServer.Context.Types;

namespace OpenApiServer.Core.MockServer.Handlers
{
    public class RequestHandlerProvider : IRequestHandlerProvider
    {
        private IDictionary<string, Type> Source { get; }
        private RequestHandlerFactory HandlerFactory { get; }

        public RequestHandlerProvider(IDictionary<string, Type> source, RequestHandlerFactory requestHandlerFactory)
        {
            Source = source;
            HandlerFactory = requestHandlerFactory;
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
            if (attr?.Options == null)
            {
                options = null;
            }
            else
            {
                var optionsType = attr.Options;
                options = Activator.CreateInstance(optionsType);
                handlerConfig.Bind(options);
            }

            return HandlerFactory.CreateHandler(handlerType, new [] {options, responseContext});
        }
    }
}