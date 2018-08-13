using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

namespace OpenApiServer.Core.MockServer.RequestHandlers.Defaults
{
    public class MockServerRequestHandlerProvider : IMockServerRequestHandlerProvider
    {
        private IDictionary<string, Type> Source { get; }
        private IServiceProvider ServiceProvider { get; }

        public MockServerRequestHandlerProvider(IServiceProvider serviceProvider, IDictionary<string, Type> source)
        {
            ServiceProvider = serviceProvider;
            Source = source;
        }

        public IMockServerRequestHandler GetHandler(string id)
        {
            Source.TryGetValue(id, out var handlerType);
            if (handlerType == null)
            {
                throw new Exception($"Unable to find handler with name '{id}'.");
            }

            return (IMockServerRequestHandler)ActivatorUtilities.CreateInstance(ServiceProvider, handlerType);
        }
    }
}