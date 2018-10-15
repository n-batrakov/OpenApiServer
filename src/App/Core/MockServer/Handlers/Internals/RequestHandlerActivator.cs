using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenApiServer.Core.MockServer.Handlers.Internals
{
    public static class RequestHandlerActivator
    {
        public static IRequestHandler CreateHandler(IServiceProvider serviceProvider, Type handlerType, object[] args)
        {
            var typeArgs = GetHandlerArgs(serviceProvider, handlerType, args).ToArray();
            return (IRequestHandler)Activator.CreateInstance(handlerType, typeArgs);
        }

        private static IEnumerable<object> GetHandlerArgs(IServiceProvider serviceProvider, Type handlerType, object[] args)
        {
            var ctor = handlerType
                       .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                       .FirstOrDefault();

            if (ctor == null)
            {
                return Enumerable.Empty<object>();
            }

            var ctorTypes = ctor.GetParameters().Select(x => x.ParameterType).ToArray();
            var ctorValues = new object[ctorTypes.Length];
            var remainingArgs = args.ToList();

            for (var i = 0; i < ctorTypes.Length; i++)
            {
                var type = ctorTypes[i];
                var value = remainingArgs.FirstOrDefault(x => x != null && type.IsInstanceOfType(x));

                if (value == default)
                {
                    value = serviceProvider.GetService(type);
                }
                else
                {
                    remainingArgs.Remove(value);
                }

                ctorValues[i] = value;
            }

            return ctorValues;
        }
    }
}