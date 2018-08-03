using System.Collections.Generic;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Core.MockServer.Context.Types
{
    public class RequestContextBody
    {
        public string ContentType { get; }
        public bool Required { get; }
        public JSchema Schema { get; }
        public IReadOnlyCollection<string> Examples { get; }

        public RequestContextBody(string contentType,
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