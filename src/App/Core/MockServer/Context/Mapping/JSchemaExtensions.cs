using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Core.MockServer.Context.Mapping
{
    public static class JSchemaExtensions
    {
        private const string ExampleKey = "x-example";

        public static void SetExample(this JSchema schema, JToken token)
        {
            schema.ExtensionData[ExampleKey] = token;
        }

        public static JToken GetExample(this JSchema schema)
        {
            schema.ExtensionData.TryGetValue(ExampleKey, out var example);
            return example;
        }
    }
}