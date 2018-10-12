using System;

using Newtonsoft.Json.Linq;

using Xunit.Sdk;

namespace UnitTests.Utils
{
    public static class JsonAssert
    {
        private const string FailMessageIntro = "JsonAssert.AreEqual failed";

        /// <summary>
        /// Asserts that two JSON strings are equals.
        /// Unlike simple strings equality, this method
        /// ignores formatting.
        /// </summary>
        /// <param name="expected">Expected JSON.</param>
        /// <param name="actual">Actual JSON.</param>
        /// <exception cref="XunitException">
        /// </exception>
        /// <remarks>
        /// Depends on Newtonsoft.Json
        /// </remarks>
        public static void Equal(string expected, string actual)
        {
            var expectedJson = GetJsonOrThrow(expected, "Expected");
            var actualJson = GetJsonOrThrow(actual, "Actual");
            if (JToken.DeepEquals(expectedJson, actualJson))
            {
                return;
            }
            throw new XunitException($"{FailMessageIntro}.\nExpected:\n{expectedJson}\n\nActual:\n{actualJson}");
        }

        private static JToken GetJsonOrThrow(string json, string nameForException)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new XunitException($"{FailMessageIntro}: '{nameForException}' is null or empty.");
            }

            var isJsonValid = TryParseJson(json, out var parsedJson);
            if (!isJsonValid)
            {
                throw new XunitException($"{FailMessageIntro}: '{nameForException}' JSON is invalid.");
            }

            return parsedJson;
        }

        private static bool TryParseJson(string json, out JToken token)
        {
            try
            {
                token = JToken.Parse(json);
                return true;
            }
            catch (Exception)
            {
                token = null;
                return false;
            }
        }
    }

}
