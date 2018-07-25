using System.Collections.Generic;
using System.Net;

using Microsoft.Extensions.Primitives;

namespace ITExpert.OpenApi.Server.Core.MockServer.Types
{
    public interface IMockServerResponseContext
    {
        HttpStatusCode StatusCode { get; }
        string Body { get; }
        string ContentType { get; }
        IDictionary<string, StringValues> Headers { get; }
    }
}