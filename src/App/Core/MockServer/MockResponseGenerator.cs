using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Server.Core.MockServer
{
    [PublicAPI]
    public class MockResponseGenerator
    {
        private OpenApiDocument Document { get; }

        public MockResponseGenerator(OpenApiDocument document)
        {
            Document = document;
        }

        public MockHttpResponse MockResponse(OpenApiResponse responseSpec, OpenApiMediaType mediaType)
        {
            var hasExample = TryGetExample(mediaType, out var responseBody);
            if (!hasExample)
            {
                var schema = ConvertSchema(mediaType.Schema);
                responseBody = GenerateFromSchema(schema).ToString();
            }
            return new MockHttpResponse(200, responseBody, new Dictionary<string, string>());
        }

        private static JSchema ConvertSchema(OpenApiSchema openApiSchema)
        {
            throw new NotImplementedException();
        }

        private static bool TryGetExample(OpenApiMediaType mediaType, out string example)
        {
            if (mediaType.Schema?.Example != null)
            {
                example = SerializeAny(mediaType.Schema.Example);
                return true;
            }

            if (mediaType.Example != null)
            {
                example = SerializeAny(mediaType.Example);
                return true;
            }

            if (mediaType.Examples?.Count > 0)
            {
                example = SerializeAny(mediaType.Examples.First().Value.Value);
                return true;
            }

            example = null;
            return false;

            string SerializeAny(IOpenApiAny any) => any.ToString();
        }

        private static JToken GenerateFromSchema(JSchema jSchema)
        {
            switch (jSchema.Type)
            {
                case JSchemaType.Object:
                    var jObject = new JObject();
                    if (jSchema.Properties == null)
                    {
                        return jObject.ToString();
                    }
                    
                    foreach (var prop in jSchema.Properties)
                    {
                        jObject.Add(prop.Key, GenerateFromSchema(prop.Value));
                    }

                    return jObject.ToString();
                case JSchemaType.Array:
                    var jArray = new JArray();
                    foreach (var item in jSchema.Items)
                    {
                        jArray.Add(GenerateFromSchema(item));
                    }

                    return jArray;
                case JSchemaType.String:
                    return new JValue("sample");
                case JSchemaType.Number:
                    return new JValue(1.0);
                case JSchemaType.Integer:
                    return new JValue(1);
                case JSchemaType.Boolean:
                    return new JValue(false);
                case JSchemaType.Null:
                case JSchemaType.None:
                case null:
                    return JValue.CreateNull();
                default:
                    return null;
            }
        }
    }
}