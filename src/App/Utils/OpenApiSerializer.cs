using System;
using System.Globalization;
using System.IO;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;

namespace ITExpert.OpenApi.Utils
{
    public static class OpenApiSerializer
    {
        public static string Serialize(Action<IOpenApiWriter> callback,
                                       OpenApiSpecVersion targetVersion = OpenApiSpecVersion.OpenApi3_0,
                                       OpenApiFormat targetFormat = OpenApiFormat.Json)
        {
            var settings = new OpenApiSerializerSettings
                           {
                                   Format = OpenApiFormat.Json,
                                   SpecVersion = targetVersion
                           };
            var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            var openApiWriter = new MyOpenApiJsonWriter(stringWriter, settings);
            callback(openApiWriter);
            return stringWriter.ToString();
        }

        public static string Serialize(IOpenApiSerializable source,
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

        private static string SerializeAsYaml(IOpenApiSerializable spec, OpenApiSpecVersion version) =>
                Serialize(x => spec.Serialize(x, version), version, OpenApiFormat.Yaml);

        private static string SerializeAsJson(IOpenApiSerializable spec, OpenApiSpecVersion version) =>
                Serialize(x => spec.Serialize(x, version), version, OpenApiFormat.Json);


        // See: https://github.com/Microsoft/OpenAPI.NET/issues/291
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
