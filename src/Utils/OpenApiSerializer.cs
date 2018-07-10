using System;
using System.IO;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Utils
{
    public static class OpenApiSerializer
    {
        public static string Serialize(OpenApiDocument source,
                                       OpenApiSpecVersion targetVersion = OpenApiSpecVersion.OpenApi3_0,
                                       OpenApiFormat targetFormat = OpenApiFormat.Json)
        {
            switch (targetFormat)
            {
                case OpenApiFormat.Json:
                    return SerializeAsJson(source, targetVersion);
                case OpenApiFormat.Yaml:
                    return SerializeAsYaml(source, targetVersion);
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetFormat), targetFormat, null);
            }
        }

        private static string SerializeAsYaml(OpenApiDocument spec, OpenApiSpecVersion version)
        {
            var settings = new OpenApiSerializerSettings
                           {
                                   Format = OpenApiFormat.Yaml,
                                   SpecVersion = version
                           };
            var stringWriter = new StringWriter();
            var openApiWriter = new OpenApiYamlWriter(stringWriter, settings);
            spec.Serialize(openApiWriter, version);
            return stringWriter.ToString();
        }

        private static string SerializeAsJson(OpenApiDocument spec, OpenApiSpecVersion version)
        {
            var settings = new OpenApiSerializerSettings
                           {
                                   Format = OpenApiFormat.Json,
                                   SpecVersion = version
                           };
            var stringWriter = new StringWriter();
            var openApiWriter = new MyOpenApiJsonWriter(stringWriter, settings);
            spec.Serialize(openApiWriter, version);
            return stringWriter.ToString();
        }

        private class MyOpenApiJsonWriter : OpenApiJsonWriter
        {
            public MyOpenApiJsonWriter(TextWriter textWriter, OpenApiSerializerSettings settings) : base(textWriter, settings)
            {
            }

            public override void WriteValue(DateTimeOffset value)
            {
                WriteValue(value.ToString("o"));
            }
        }
    }
}
