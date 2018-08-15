using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace OpenApiServer.Core.MockServer.Handlers
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

        public IRequestHandler GetHandler(string id, params object[] args)
        {
            Source.TryGetValue(id, out var handlerType);
            if (handlerType == null)
            {
                throw new Exception($"Unable to find handler with name '{id}'.");
            }

            var typeArgs = GetHandlerArgs(handlerType, args).ToArray();
            
            return (IRequestHandler)ActivatorUtilities.CreateInstance(ServiceProvider, handlerType, typeArgs);
        }

        private static IEnumerable<object> GetHandlerArgs(Type handlerType, object[] args)
        {
            var ctor = handlerType
                       .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                       .FirstOrDefault();

            if (ctor == null)
            {
                return Enumerable.Empty<object>();
            }

            var ctorTypes = ctor.GetParameters().Select(x => x.ParameterType).ToArray();
            return args.Where(IsCtorArgument);


            bool IsCtorArgument(object arg)
            {
                var type = arg.GetType();
                return ctorTypes.Any(x => x.IsAssignableFrom(type));
            }
        }
    }
}