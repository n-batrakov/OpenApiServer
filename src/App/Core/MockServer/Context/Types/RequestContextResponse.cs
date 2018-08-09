using System;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json.Schema;

namespace OpenApiServer.Core.MockServer.Context.Types
{
    public class RequestContextResponse
    {
        public string ContentType { get; }
        public string StatusCode { get; }
        public JSchema Schema { get; }
        public IReadOnlyCollection<string> Examples { get; }

        public HttpStatusCode StatusCodeParsed =>
                StatusCode.Equals("default", StringComparison.OrdinalIgnoreCase)
                        ? HttpStatusCode.InternalServerError
                        : (HttpStatusCode)int.Parse(StatusCode);

        public RequestContextResponse(string contentType,
                                      string statusCode,
                                      JSchema schema,
                                      IReadOnlyCollection<string> examples)
        {
            ContentType = contentType;
            StatusCode = statusCode;
            Schema = schema;
            Examples = examples;
        }
    }
}