using System;

namespace OpenApiServer.Core.MockServer.Handlers
{
    public class RequestHandlerAttribute : Attribute
    {
        public string HandlerId { get; }
        public Type Options { get; }

        public RequestHandlerAttribute(string handlerId)
        {
            HandlerId = handlerId;
        }

        public RequestHandlerAttribute(string handlerId, Type options)
        {
            HandlerId = handlerId;
            Options = options;
        }
    }
}