using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenApiServer.Core.MockServer.Handlers.Internals
{
    public class RequestHandlerActivator
    {
        private IServiceProvider ServiceProvider { get; }

        public RequestHandlerActivator(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IRequestHandler CreateHandler(Type handlerType, object[] args)
        {
            var typeArgs = GetHandlerArgs(handlerType, args).ToArray();
            return (IRequestHandler)Activator.CreateInstance(handlerType, typeArgs);
        }

        private IEnumerable<object> GetHandlerArgs(Type handlerType, object[] args)
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

                if (value == null)
                {
                    value = ServiceProvider.GetService(type);
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