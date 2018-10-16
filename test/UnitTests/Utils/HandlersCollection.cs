using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using OpenApiServer.Core.MockServer.Context;
using OpenApiServer.Core.MockServer.Handlers;

namespace UnitTests.Utils
{
    public class HandlersCollection
    {
        public IReadOnlyCollection<string> Keys { get; }
        public RequestHandlerProvider HandlerProvider { get; }

        public HandlersCollection(params Type[] handlers) : this(GetHandlerMap(handlers))
        {
        }

        public HandlersCollection(IReadOnlyDictionary<string, Type> source)
        {
            Keys = source.Keys.ToArray();

            var handlers = new Dictionary<string, Type>(source);
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            HandlerProvider = new RequestHandlerProvider(handlers, serviceProvider);
        }

        private static IReadOnlyDictionary<string, Type> GetHandlerMap(IEnumerable<Type> handlerTypes) =>
                handlerTypes.ToDictionary(x => x.GetCustomAttribute<RequestHandlerAttribute>().HandlerId);
    }
}