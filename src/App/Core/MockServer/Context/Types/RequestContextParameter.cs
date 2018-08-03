using System.Collections.Generic;

using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Core.MockServer.Context.Types
{
    public class RequestContextParameter
    {
        public string Name { get; set; }
        public ParameterLocation In { get; set; }
        public bool Explode { get; set; }
        public ParameterStyle? Style { get; set; }
        public bool AllowEmptyValue { get; set; }

        public bool Required { get; set; }
        public JSchema Schema { get; set; }

        public IReadOnlyCollection<string> Examples { get; set; }
    }
}