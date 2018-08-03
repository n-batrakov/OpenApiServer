using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Core.MockServer.Generation.Generators
{
    public class EnumGenerator : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public EnumGenerator(Random random)
        {
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            var isEnum = schema.Enum != null && schema.Enum.Count > 0;
            if (!isEnum)
            {
                return false;
            }

            var valueIndex = Random.Next(0, schema.Enum.Count);
            var value = schema.Enum[valueIndex];
            WriteJToken(writer, value);

            return true;
        }

        public void WriteJToken(IOpenApiWriter writer, JToken token)
        {
            switch (token.Type)
            {
                
                case JTokenType.String:
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.TimeSpan:
                case JTokenType.Uri:
                    writer.WriteValue(token.ToString());
                    break;
                default:
                    writer.WriteRaw(token.ToString());
                    break;
            }
        }
    }
}