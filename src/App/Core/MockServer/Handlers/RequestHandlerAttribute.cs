using System;

namespace OpenApiServer.Core.MockServer.Handlers
{
    public class RequestHandlerAttribute : Attribute
    {
        public string HandlerId { get; }

        public RequestHandlerAttribute(string handlerId)
        {
            HandlerId = handlerId;
        }
    }
}