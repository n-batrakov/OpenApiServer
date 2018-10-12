using System.Collections.Generic;

using Newtonsoft.Json.Schema;

namespace OpenApiServer.Core.MockServer.Context.Types.Spec
{
    public class RouteSpecRequestBody
    {
        public string ContentType { get; }
        public bool Required { get; }
        public JSchema Schema { get; }
        public IReadOnlyCollection<string> Examples { get; }

        public RouteSpecRequestBody(string contentType,
                                  bool required,
                                  JSchema schema,
                                  IReadOnlyCollection<string> examples)
        {
            ContentType = contentType;
            Required = required;
            Schema = schema;
            Examples = examples;
        }
    }
}