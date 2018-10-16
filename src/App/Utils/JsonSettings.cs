using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenApiServer.Utils
{
    public static class JsonSettings
    {
        public static readonly JsonSerializerSettings Value = 
                new JsonSerializerSettings
                {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
    }
}
