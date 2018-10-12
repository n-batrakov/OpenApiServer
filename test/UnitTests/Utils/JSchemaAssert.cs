using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using Xunit.Sdk;

namespace UnitTests.Utils
{
    public static class JSchemaAssert
    {
        public static void Match(JSchema schema, string json)
        {
            var token = JToken.Parse(json);
            var isValid = token.IsValid(schema, out IList<string> errors);
            if (isValid)
            {
            }
            else
            {
                var msg = string.Join("\r\n", errors);
                throw new XunitException($"JSON value does not match the schema.\r\n{msg}");
            }
        }
    }
}