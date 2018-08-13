using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

namespace OpenApiServer.Core.MockServer.Handlers.Defaults
{
    public class RequestHandlerProvider : IRequestHandlerProvider
    {
        private IDictionary<string, Type> Source { get; }
        private IServiceProvider ServiceProvider { get; }

        public RequestHandlerProvider(IServiceProvider serviceProvider, IDictionary<string, Type> source)
        {
            ServiceProvider = serviceProvider;
            Source = source;
        }

        public IRequestHandler GetHandler(string id)
        {
            Source.TryGetValue(id, out var handlerType);
            if (handlerType == null)
            {
                throw new Exception($"Unable to find handler with name '{id}'.");
            }

            return (IRequestHandler)ActivatorUtilities.CreateInstance(ServiceProvider, handlerType);
        }
    }
}