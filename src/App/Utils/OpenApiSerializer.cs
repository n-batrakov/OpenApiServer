using System;
using System.Globalization;
using System.IO;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;

namespace OpenApiServer.Utils
{
    public static class OpenApiSerializer
    {
        public static string Serialize(Action<IOpenApiWriter> callback)
        {
            var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            var openApiWriter = new MyOpenApiWriter(stringWriter);
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
                    return Serialize(x => source.Serialize(x, targetVersion));
                case OpenApiFormat.Yaml:
                    return source.SerializeAsYaml(targetVersion);
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetFormat), targetFormat, null);
            }
        }

        private class MyOpenApiWriter : OpenApiJsonWriter
        {
            public MyOpenApiWriter(TextWriter textWriter)
                    : base(textWriter)
            {
            }

            public override void WriteValue(DateTimeOffset value)
            {
                WriteValue(value.ToString("o"));
            }
        }
    }
}
